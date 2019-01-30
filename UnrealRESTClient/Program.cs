using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace UnrealRESTClient
{
    class Property
    {
        //name, type
        public Dictionary<string, string> property;

        public Property()
        {
            property = new Dictionary<string, string>();
        }
    }

   

    class Program
    {
        static void ParseSchema(OpenApiSchema InSchema, ref Dictionary<string, Property> OutProperties, string ObjectKey)
        {
            if(InSchema.Type == "object")
            {
                if(!OutProperties.ContainsKey(ObjectKey))
                {
                    Property p = new Property();
                    OutProperties.Add(ObjectKey, p);
                    ParseSchema(InSchema, ref OutProperties, ObjectKey);
                }
                
            }
            foreach (var props in InSchema.Properties)
            {
                if (props.Value.Type == "object")
                {
                    Property p = new Property();
                    if(OutProperties.ContainsKey(props.Key))
                    {
                        //can't contain itself.
                        if(!OutProperties[props.Key].property.ContainsKey(props.Key))
                        {
                            OutProperties[props.Key].property.Add(props.Key, props.Value.Type);
                        }
                    }
                    else
                    {
                        OutProperties.Add(props.Key, p);
                        ParseSchema(props.Value, ref OutProperties, props.Key);
                    }
                    
                }
                else
                {
                    if(OutProperties[ObjectKey].property.ContainsKey(props.Key))
                    {

                    }
                    else
                    {
                        OutProperties[ObjectKey].property.Add(props.Key, props.Value.Type);
                    }
                }
            }
        }

        static void ExtractObjects(OpenApiSchema InSchema, ref Dictionary<string, Property> OutProperties, string ObjectKey)
        {
            if (InSchema.Type == "object")
            {
                if (!OutProperties.ContainsKey(ObjectKey))
                {
                    Property p = new Property();
                    OutProperties.Add(ObjectKey, p);
                    ParseSchema(InSchema, ref OutProperties, ObjectKey);
                }

            }
            foreach (var props in InSchema.Properties)
            {
                if (props.Value.Type == "object")
                {
                    
                    if (OutProperties.ContainsKey(props.Key))
                    {
                    }
                    else
                    {
                        Property p = new Property();
                        OutProperties.Add(props.Key, p);
                        ParseSchema(props.Value, ref OutProperties, props.Key);
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://raw.githubusercontent.com/")
            };

            var stream = httpClient.GetStreamAsync("heroiclabs/nakama/master/apigrpc/apigrpc.swagger.json").Result;

            var openApiDocument = new OpenApiStreamReader().Read(stream, out var diagnostic);

            var path = openApiDocument.Paths;


            var schemas = openApiDocument.Components.Schemas;
            
            HashSet<string> set = new HashSet<string>();

            Dictionary<string, Property> objects = new Dictionary<string, Property>();

            //objects which do not contain oher objects as properties
            List<string> SimpleObjects = new List<string>();
            //objects which have other objects as properties.
            List<string> ComplexObjects = new List<string>();

            //cherck down to properties, to see if they are linking some other object
            foreach (var schema in schemas)
            {
                //ParseSchema(schema.Value, ref objects, schema.Key);
                //ExtractObjects(schema.Value, ref objects, schema.Key);
                if (schema.Value.Reference != null)
                {
                    ComplexObjects.Add(schema.Key);
                }
                else
                {
                    SimpleObjects.Add(schema.Key);
                }
            }

            foreach(KeyValuePair<string, Property> obj in objects)
            {
            }

            foreach (var schema in schemas)
            {
                foreach(var prop in schema.Value.Properties)
                {
                    if(objects.ContainsKey(prop.Key))
                    {
                        if(objects.ContainsKey(schema.Key))
                        {
                            //if(!objects.ContainsKey(prop.Key))
                            {
                                objects[schema.Key].property.Add(prop.Key, prop.Value.Type);
                            }
                            
                        }
                    }
                }
                
            }

            if (objects.Count > 0)
            {

            }
        }
    }
}
