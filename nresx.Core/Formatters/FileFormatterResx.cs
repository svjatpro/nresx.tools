using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace nresx.Tools.Formatters
{
    internal class FileFormatterResx : IFileFormatter
    {
        #region Private members

        private const string RootDescription = @"
    Microsoft ResX Schema 
    
    Version 2.0
    
    The primary goals of this format is to allow a simple XML format 
    that is mostly human readable. The generation and parsing of the 
    various data types are done through the TypeConverter classes 
    associated with the data types.
    
    Example:
    
    ... ado.net/XML headers & schema ...
    <resheader name=""resmimetype"">text/microsoft-resx</resheader>
    <resheader name=""version"">2.0</resheader>
    <resheader name=""reader"">System.Resources.ResXResourceReader, System.Windows.Forms, ...</resheader>
    <resheader name=""writer"">System.Resources.ResXResourceWriter, System.Windows.Forms, ...</resheader>
    <data name=""Name1""><value>this is my long string</value><comment>this is a comment</comment></data>
    <data name=""Color1"" type=""System.Drawing.Color, System.Drawing"">Blue</data>
    <data name=""Bitmap1"" mimetype=""application/x-microsoft.net.object.binary.base64"">
        <value>[base64 mime encoded serialized .NET Framework object]</value>
    </data>
    <data name=""Icon1"" type=""System.Drawing.Icon, System.Drawing"" mimetype=""application/x-microsoft.net.object.bytearray.base64"">
        <value>[base64 mime encoded string representing a byte array form of the .NET Framework object]</value>
        <comment>This is a comment</comment>
    </data>
                
    There are any number of ""resheader"" rows that contain simple 
    name/value pairs.
    
    Each data row contains a name, and value. The row also contains a 
    type or mimetype. Type corresponds to a .NET class that support 
    text/value conversion through the TypeConverter architecture. 
    Classes that don't support this are serialized and stored with the 
    mimetype set.
    
    The mimetype is used for serialized objects, and tells the 
    ResXResourceReader how to depersist the object. This is currently not 
    extensible. For a given mimetype the value must be set accordingly:
    
    Note - application/x-microsoft.net.object.binary.base64 is the format 
    that the ResXResourceWriter will generate, however the reader can 
    read any of the formats listed below.
    
    mimetype: application/x-microsoft.net.object.binary.base64
    value   : The object must be serialized with 
            : System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
            : and then encoded with base64 encoding.
    
    mimetype: application/x-microsoft.net.object.soap.base64
    value   : The object must be serialized with 
            : System.Runtime.Serialization.Formatters.Soap.SoapFormatter
            : and then encoded with base64 encoding.

    mimetype: application/x-microsoft.net.object.bytearray.base64
    value   : The object must be serialized into a byte array 
            : using a System.ComponentModel.TypeConverter
            : and then encoded with base64 encoding.
    ";

        private XElement GetSchema()
        {
            XNamespace xsd = "http://www.w3.org/2001/XMLSchema";
            XNamespace msdata = "urn:schemas-microsoft-com:xml-msdata";

            var schema = new XElement( xsd + "schema",
                new XAttribute( "id", "root" ),
                new XAttribute( "xmlns", "" ),
                new XAttribute( XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema" ),
                new XAttribute( XNamespace.Xmlns + "msdata", "urn:schemas-microsoft-com:xml-msdata" ),
                new XElement( xsd + "import", new XAttribute( "namespace", "http://www.w3.org/XML/1998/namespace" ) ),
                new XElement( xsd + "element",
                    new XAttribute( "name", "root" ),
                    new XAttribute( msdata + "IsDataSet", "true" ),
                    new XElement( xsd + "complexType",
                        new XElement( xsd + "choice", new XAttribute( "maxOccurs", "unbounded" ),
                            new XElement( xsd + "element", new XAttribute( "name", "metadata" ),
                                new XElement( xsd + "complexType",
                                    new XElement( xsd + "sequence",
                                        new XElement( xsd + "element",
                                            new XAttribute( "name", "value" ),
                                            new XAttribute( "type", "xsd:string" ),
                                            new XAttribute( "minOccurs", "0" ) ) ),
                                    new XElement( xsd + "attribute",
                                        new XAttribute( "name", "name" ),
                                        new XAttribute( "use", "required" ),
                                        new XAttribute( "type", "xsd:string" ) ),
                                    new XElement( xsd + "attribute",
                                        new XAttribute( "name", "type" ),
                                        new XAttribute( "type", "xsd:string" ) ),
                                    new XElement( xsd + "attribute",
                                        new XAttribute( "name", "mimetype" ),
                                        new XAttribute( "type", "xsd:string" ) ),
                                    new XElement( xsd + "attribute", new XAttribute( "ref", "xml:space" ) ) ) ),
                            new XElement( xsd + "element", new XAttribute( "name", "assembly" ),
                                new XElement( xsd + "complexType",
                                    new XElement( xsd + "attribute", new XAttribute( "name", "alias" ),
                                        new XAttribute( "type", "xsd:string" ) ),
                                    new XElement( xsd + "attribute", new XAttribute( "name", "name" ),
                                        new XAttribute( "type", "xsd:string" ) ) ) ),
                            new XElement( xsd + "element", new XAttribute( "name", "data" ),
                                new XElement( xsd + "complexType",
                                    new XElement( xsd + "sequence",
                                        new XElement( xsd + "element",
                                            new XAttribute( "name", "value" ),
                                            new XAttribute( "type", "xsd:string" ),
                                            new XAttribute( "minOccurs", "0" ),
                                            new XAttribute( msdata + "Ordinal", "1" ) ),
                                        new XElement( xsd + "element",
                                            new XAttribute( "name", "comment" ),
                                            new XAttribute( "type", "xsd:string" ),
                                            new XAttribute( "minOccurs", "0" ),
                                            new XAttribute( msdata + "Ordinal", "2" ) ) ),
                                    new XElement( xsd + "attribute",
                                        new XAttribute( "name", "name" ),
                                        new XAttribute( "type", "xsd:string" ),
                                        new XAttribute( "use", "required" ),
                                        new XAttribute( msdata + "Ordinal", "1" ) ),
                                    new XElement( xsd + "attribute",
                                        new XAttribute( "name", "type" ),
                                        new XAttribute( "type", "xsd:string" ),
                                        new XAttribute( msdata + "Ordinal", "3" ) ),
                                    new XElement( xsd + "attribute",
                                        new XAttribute( "name", "mimetype" ),
                                        new XAttribute( "type", "xsd:string" ),
                                        new XAttribute( msdata + "Ordinal", "4" ) ),
                                    new XElement( xsd + "attribute", new XAttribute( "ref", "xml:space" ) ) ) ),
                            new XElement( xsd + "element", new XAttribute( "name", "resheader" ),
                                new XElement( xsd + "complexType",
                                    new XElement( xsd + "sequence",
                                        new XElement( xsd + "element",
                                            new XAttribute( "name", "value" ),
                                            new XAttribute( "type", "xsd:string" ),
                                            new XAttribute( "minOccurs", "0" ),
                                            new XAttribute( msdata + "Ordinal", "1" ) ) ),
                                    new XElement( xsd + "attribute",
                                        new XAttribute( "name", "name" ),
                                        new XAttribute( "type", "xsd:string" ),
                                        new XAttribute( "use", "required" ) ) ) ) ) ) ) );
            return schema;
        }

        #endregion

        public bool LoadResourceFile( Stream stream, out IEnumerable<ResourceElement> elements )
        {
            if ( LoadRawElements( stream, out var raw ) )
            {
                var dictionary = new Dictionary<string, ResourceElement>();
                foreach ( var el in raw )
                {
                    if( !dictionary.ContainsKey( el.Key ) )
                        dictionary.Add( el.Key, el );
                }
                elements = dictionary.Values.ToList();
                return true;
            }

            elements = null;
            return false;
        }

        public bool LoadRawElements( Stream stream, out IEnumerable<ResourceElement> elements )
        {
            var doc = XDocument.Load( stream );
            var entries = doc.Root?.Elements( "data" );

            elements = entries?
                .Select( e => new ResourceElement
                {
                    Key = e.Attribute( "name" )?.Value,
                    Value = e.Element( "value" )?.Value,
                    Comment = e.Element( "comment" )?.Value
                } )
                .ToList();

            return true;
        }

        public void SaveResourceFile( Stream stream, IEnumerable<ResourceElement> elements, bool validate = true )
        {
            XNamespace xml = "http://www.w3.org/XML/1998/namespace";
            var doc = new XDocument(
                new XElement( "root",

                    // write root description
                    new XComment( RootDescription ),

                    // write schema
                    GetSchema(),

                    // write headers
                    new XElement( "resheader",
                        new XAttribute( "name", "resmimetype" ),
                        new XElement( "value", "text/microsoft-resx" ) ),
                    new XElement( "resheader",
                        new XAttribute( "name", "version" ),
                        new XElement( "value", "2.0" ) ),
                    new XElement( "resheader", 
                        new XAttribute( "name", "reader" ),
                        new XElement( "value", "System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" ) ),
                    new XElement( "resheader",
                        new XAttribute( "name", "writer" ),
                        new XElement( "value", "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" ) ),
                    
                    // write elements
                    elements.Select( el =>
                    {
                        var elElements = new List<object>
                        {
                            new XAttribute( "name", el.Key ),
                            new XAttribute( xml + "space", "preserve" ),
                            new XElement( "value", el.Value )
                        };
                        if( !string.IsNullOrWhiteSpace( el.Comment ) )
                            elElements.Add( new XElement( "comment", el.Comment.ToArray() ) );

                        var xel = new XElement( "data", elElements );
                        return xel;
                    } ) ) );

            doc.Save( stream );
        }
    }
}