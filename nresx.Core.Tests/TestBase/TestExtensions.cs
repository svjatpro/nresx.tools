using System;
using System.Collections.Generic;

namespace nresx.Core.Tests
{
    public class CommandRunContext
    {
        public string CommandLine { get; set; }
        public Func<CommandLineParameters> PredefinedParameters { get; set; }
        public readonly List<CommandLineParameters> CommandRunResults = new();
    }
    public class CommandRunContext<T> : CommandRunContext
    {
        public Func<CommandLineParameters, T> AdditionalParamsBuilder { get; set; }
    }

    //commandLine
    //    .WithParams( args => new
    //    {
    //        SrcPath = args.SourceFiles[0].GetShortPath(),
    //        NewPath = args.NewFiles[0].GetShortPath()
    //    } )
    //    .ValidateRun( ( args, parameters ) =>
    //    {
    //        var res = new ResourceFile( parameters.NewPath );
    //        res.FileFormat.Should().Be( type );
    //        ValidateElements( res );
    //    } )
    //    .ValidateDryRun( ( args, parameters ) =>
    //    {
    //        new FileInfo( parameters.NewPath ).Exists.Should().BeFalse();
    //    } )
    //    .ValidateStdout( (args, parameters) => new[]
    //    {
    //        string.Format( SuccessLineTemplate, parameters.SrcPath, parameters.NewPath )
    //    } );

    public static class TestExtensions
    {
        public static CommandRunContext<T> WithParams<T>( this string cmdLine, Func<CommandLineParameters, T> builder )
            where T : class
        {
            return new CommandRunContext<T>
            {
                CommandLine = cmdLine,
                AdditionalParamsBuilder = builder
            };
        }
        public static CommandRunContext<T> WithParams<T>( this CommandRunContext context, Func<CommandLineParameters, T> builder )
            where T : class
        {
            if( !( context is CommandRunContext<T> newContext ) )
            { 
                newContext = new CommandRunContext<T>
                {
                    CommandLine = context.CommandLine,
                    PredefinedParameters = context.PredefinedParameters
                };
                context.CommandRunResults.AddRange( context.CommandRunResults );
            }

            newContext.AdditionalParamsBuilder = builder;
            return newContext;
        }
        public static CommandRunContext<T> WithParams<T>( this CommandRunContext<T> context, Func<CommandLineParameters, T> builder )
            where T : class
        {
            context.AdditionalParamsBuilder = builder;
            return context;
        }


        public static CommandRunContext PrepareArgs( this string cmdLine, Func<CommandLineParameters> builder )
        {
            return new CommandRunContext
            {
                CommandLine = cmdLine,
                PredefinedParameters = builder
            };
        }
        public static CommandRunContext<T> PrepareArgs<T>( this CommandRunContext<T> context, Func<CommandLineParameters> builder )
        {
            context.PredefinedParameters = builder;
            return context;
        }

        public static CommandRunContext ValidateRun( this string commandLine, Action<CommandLineParameters> validateAction )
        {
            return new CommandRunContext {CommandLine = commandLine}.ValidateRun( validateAction );
        }
        public static CommandRunContext ValidateRun( this CommandRunContext context, Action<CommandLineParameters> validateAction )
        {
            var result = TestHelper.RunCommandLine( context.CommandLine, context.PredefinedParameters?.Invoke(), mergeArgs: true );

            Console.WriteLine( "run command:" );
            Console.WriteLine( $"\"{result.CommandLine}\"" );
            Console.WriteLine( new string( '-', 20 ) );
            foreach ( var line in result.ConsoleOutput )
                Console.WriteLine( line );
            Console.WriteLine( new string( '=', 50 ) );

            validateAction( result );
            
            context.CommandRunResults.Add( result );
            return context;
        }
        public static CommandRunContext<T> ValidateRun<T>( this CommandRunContext<T> context, Action<CommandLineParameters, T> validateAction )
        {
            return context
                .ValidateRun( args =>
                {
                    var parameters = context.AdditionalParamsBuilder( args );
                    validateAction( args, parameters );
                } ) as CommandRunContext<T>;
        }


        public static CommandRunContext ValidateDryRun( this string commandLine, Action<CommandLineParameters> validateAction )
        {
            return new CommandRunContext {CommandLine = commandLine}.ValidateDryRun( validateAction );
        }
        public static CommandRunContext ValidateDryRun( this CommandRunContext context, Action<CommandLineParameters> validateAction )
        {
            var cmdLine = context.CommandLine;
            if ( !cmdLine.Contains( TestData.DryRunOption ) )
                cmdLine = $"{cmdLine} {TestData.DryRunOption}";

            var result = TestHelper.RunCommandLine( cmdLine, context.PredefinedParameters?.Invoke(), mergeArgs: true );

            Console.WriteLine( "run command:" );
            Console.WriteLine( $"\"{result.CommandLine}\"" );
            Console.WriteLine( new string( '-', 20 ) );
            foreach ( var line in result.ConsoleOutput )
                Console.WriteLine( line );
            Console.WriteLine( new string( '=', 50 ) );

            validateAction( result );

            context.CommandRunResults.Add( result );
            return context;
        }
        public static CommandRunContext<T> ValidateDryRun<T>( this CommandRunContext<T> context, Action<CommandLineParameters, T> validateAction )
        {
            return context
                .ValidateDryRun( args =>
                {
                    var parameters = context.AdditionalParamsBuilder( args );
                    validateAction( args, parameters );
                } ) as CommandRunContext<T>;
        }
        
        
        public static CommandRunContext ValidateStdout( this CommandRunContext context, Func<CommandLineParameters, string[]> validateAction )
        {
            foreach ( var args in context.CommandRunResults )
            {
                Console.WriteLine( "validate output for command:" );
                Console.WriteLine( $"\"{args.CommandLine}\"" );
                Console.WriteLine( new string( '-', 20 ) );
                foreach ( var line in args.ConsoleOutput )
                    Console.WriteLine( line );
                Console.WriteLine( new string( '=', 50 ) );

                validateAction( args );
            }
            
            return context;
        }
        public static CommandRunContext<T> ValidateStdout<T>( this CommandRunContext<T> context, Func<CommandLineParameters, T, string[]> validateAction )
        {
            foreach ( var args in context.CommandRunResults )
            {
                Console.WriteLine( "validate output for command:" );
                Console.WriteLine( $"\"{args.CommandLine}\"" );
                Console.WriteLine( new string( '-', 20 ) );
                foreach ( var line in args.ConsoleOutput )
                    Console.WriteLine( line );
                Console.WriteLine( new string( '=', 50 ) );

                var parameters = context.AdditionalParamsBuilder( args );

                validateAction( args, parameters );
            }

            return context;
        }
    }
}