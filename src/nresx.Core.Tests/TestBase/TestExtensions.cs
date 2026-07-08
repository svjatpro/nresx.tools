using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace nresx.Core.Tests
{
    public class CommandsStatistics
    {
        public static ConcurrentBag<string> Commands = [];
    }
    public class CommandRunContext
    {
        public string CommandLine { get; set; }
        public Action<CommandLineParameters> BeforeRunAction { get; set; }
        public Func<CommandLineParameters> PredefinedParameters { get; set; }
        public Func<CommandRunOptions> CommandOptions { get; set; } = () => new CommandRunOptions { MergeArgs = true };

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
        #region WithParams

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
                    PredefinedParameters = context.PredefinedParameters,
                    CommandOptions = context.CommandOptions
                };
                context.CommandRunResults.AddRange( context.CommandRunResults );
            }

            newContext.AdditionalParamsBuilder = builder;
            return newContext;
        }

        #endregion

        public static CommandRunContext BeforeRun( this CommandRunContext context, Action action )
        {
            context.BeforeRunAction = args => action();
            return context;
        }

        public static CommandRunContext BeforeRun( this CommandRunContext context, Action<CommandLineParameters> action )
        {
            context.BeforeRunAction = action;
            return context;
        }

        #region WithOptions

        public static CommandRunContext WithOptions( this string cmdLine, Func<CommandRunOptions> optionsBuilder )
        {
            return new CommandRunContext
            {
                CommandLine = cmdLine,
                CommandOptions = optionsBuilder
            };
        }
        public static CommandRunContext WithOptions( this string cmdLine, Action<CommandRunOptions> optionsBuilder )
        {
            return WithOptions( new CommandRunContext { CommandLine = cmdLine }, optionsBuilder );
        }

        public static CommandRunContext WithOptions( this CommandRunContext context, Func<CommandRunOptions> optionsBuilder )
        {
            context.CommandOptions = optionsBuilder;
            return context;
        }
        public static CommandRunContext WithOptions( this CommandRunContext context, Action<CommandRunOptions> optionsBuilder )
        {
            context.CommandOptions = () =>
            {
                var opt = new CommandRunOptions { MergeArgs = true };
                optionsBuilder( opt );
                return opt;
            };
            return context;
        }

        public static CommandRunContext<T> WithOptions<T>( this CommandRunContext<T> context, Func<CommandRunOptions> optionsBuilder )
            where T : class
        {
            context.CommandOptions = optionsBuilder;
            return context;
        }
        public static CommandRunContext<T> WithOptions<T>( this CommandRunContext<T> context, Action<CommandRunOptions> optionsBuilder )
            where T : class
        {
            WithOptions( context as CommandRunContext, optionsBuilder );
            return context;
        }

        #endregion

        #region PrepareArgs

        /// <summary>
        /// Prepare all generic variables in command line
        /// </summary>
        public static CommandRunContext PrepareArgs( this string cmdLine, Func<CommandLineParameters> builder )
        {
            return new CommandRunContext
            {
                CommandLine = cmdLine,
                PredefinedParameters = builder
            };
        }

        public static CommandRunContext PrepareArgs( this CommandRunContext context, Func<CommandLineParameters> builder )
        {
            context.PredefinedParameters = builder;
            return context;
        }

        public static CommandRunContext<T> PrepareArgs<T>( this CommandRunContext<T> context, Func<CommandLineParameters> builder )
        {
            context.PredefinedParameters = builder;
            return context;
        }

        #endregion

        #region Run

        public static CommandRunContext Run( this string commandLine )
        {
            return commandLine.ValidateRun( () => { } );
        }

        public static CommandRunContext Run( this CommandRunContext context )
        {
            return context.ValidateRun( () => { } );
        }

        #endregion

        #region ValidateRun

        public static CommandRunContext ValidateRun( this string commandLine, Action<CommandLineParameters> validateAction )
        {
            return new CommandRunContext {CommandLine = commandLine}.ValidateRun( validateAction );
        }
        public static CommandRunContext ValidateRun( this string commandLine, Action validateAction )
        {
            return new CommandRunContext { CommandLine = commandLine }.ValidateRun( validateAction );
        }
        
        public static CommandRunContext ValidateRun( this CommandRunContext context, Action<CommandLineParameters> validateAction )
        {
            var preArgs = context.PredefinedParameters?.Invoke();
            var options = context.CommandOptions?.Invoke();
            var commandLine = TestHelper.PrepareCommandLine( context.CommandLine, out var args, preArgs, options );

            context.BeforeRunAction?.Invoke( args );
            var result = TestHelper.RunCommandLine( commandLine, args, options );

            CommandsStatistics.Commands.Add( result.CommandLine );

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
        public static CommandRunContext ValidateRun( this CommandRunContext context, Action validateAction )
        {
            return ValidateRun( context, args => validateAction() );
        }

        public static CommandRunContext<T> ValidateRun<T>( this CommandRunContext<T> context, Action<CommandLineParameters, T> validateAction )
        {
            return context
                .ValidateRun( args =>
                {
                    var parameters = context.AdditionalParamsBuilder( args );
                    validateAction( args, parameters );
                } );
        }
        public static CommandRunContext<T> ValidateRun<T>( this CommandRunContext<T> context, Action<CommandLineParameters> validateAction )
        {
            return (context as CommandRunContext).ValidateRun( validateAction ) as CommandRunContext<T>;
        }
        public static CommandRunContext<T> ValidateRun<T>( this CommandRunContext<T> context, Action validateAction )
        {
            return ( context as CommandRunContext )
                .ValidateRun( args =>
                {
                    context.AdditionalParamsBuilder?.Invoke( args );
                    validateAction();
                } ) as CommandRunContext<T>;
        }

        #endregion

        #region ValidateDryRun

        public static CommandRunContext ValidateDryRun( this string commandLine, Action<CommandLineParameters> validateAction )
        {
            return new CommandRunContext {CommandLine = commandLine}.ValidateDryRun( validateAction );
        }
        public static CommandRunContext ValidateDryRun( this string commandLine, Action validateAction )
        {
            return new CommandRunContext { CommandLine = commandLine }.ValidateDryRun( validateAction );
        }
        
        public static CommandRunContext ValidateDryRun( this CommandRunContext context, Action<CommandLineParameters> validateAction )
        {
            var cmdLine = context.CommandLine;
            if ( !cmdLine.Contains( TestData.DryRunOption ) )
                cmdLine = $"{cmdLine} {TestData.DryRunOption}";

            var preArgs = context.PredefinedParameters?.Invoke();
            var options = context.CommandOptions?.Invoke();
            var commandLine = TestHelper.PrepareCommandLine( cmdLine, out var args, preArgs, options );

            context.BeforeRunAction?.Invoke( args );
            var result = TestHelper.RunCommandLine( commandLine, args, options );

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
        public static CommandRunContext ValidateDryRun( this CommandRunContext context, Action validateAction )
        {
            return ValidateDryRun( context, args => validateAction() );
        }

        public static CommandRunContext<T> ValidateDryRun<T>( this CommandRunContext<T> context, Action<CommandLineParameters, T> validateAction )
        {
            return context
                .ValidateDryRun( args =>
                {
                    var parameters = context.AdditionalParamsBuilder( args );
                    validateAction( args, parameters );
                } );
        }
        public static CommandRunContext<T> ValidateDryRun<T>( this CommandRunContext<T> context, Action<CommandLineParameters> validateAction )
        {
            return (context as CommandRunContext).ValidateDryRun( validateAction ) as CommandRunContext<T>;
        }
        public static CommandRunContext<T> ValidateDryRun<T>( this CommandRunContext<T> context, Action validateAction )
        {
            return ( context as CommandRunContext ).ValidateDryRun( validateAction ) as CommandRunContext<T>;
        }

        #endregion

        #region ValidateStdout

        public static CommandRunContext ValidateStdout( this CommandRunContext context, Action<CommandLineParameters> validateAction )
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
        public static CommandRunContext<T> ValidateStdout<T>( this CommandRunContext<T> context, Action<CommandLineParameters, T> validateAction )
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

        #endregion
    }
}