using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace UnrealRESTClient
{
    public class GenObject
    {
        public string Name;
        public string KeyName;
        public List<GenObject> DependsOn;
    }

    public class PropertyBase
    {
        public string Name; //name
    }

    public class Property : PropertyBase
    {
        public string GenType; //an actual type that is generated
        public string Type; //primitive type
    }

    public class ObjectProperty : PropertyBase
    {
        public string ReferenceName;
        public string TypeName;
        public List<PropertyBase> Properties;
    }

    

    class Program
    {
        static void BuildObjects(ref List<ObjectProperty> Objects, IDictionary<string, OpenApiSchema> Schemas)
        {
            foreach (KeyValuePair<string, OpenApiSchema> s in Schemas)
            {
                if (s.Value.Reference != null)
                {
                    ObjectProperty o = new ObjectProperty
                    {
                        Name = s.Key,
                        ReferenceName = s.Value.Reference.ReferenceV3,
                        TypeName = s.Value.Reference.Id,
                        Properties = new List<PropertyBase>()
                    };

                    foreach (KeyValuePair<string, OpenApiSchema> p in s.Value.Properties)
                    {
                        if (p.Value.Type == "object")
                        {
                            ObjectProperty dob = new ObjectProperty
                            {
                                Name = p.Key,
                                ReferenceName = p.Value.Reference.ReferenceV3,
                                TypeName = p.Value.Reference.Id,
                                Properties = new List<PropertyBase>()
                            };
                            o.Properties.Add(dob);

                            BuildObjects(ref Objects, p.Value.Properties);
                        }
                        else
                        {
                            Property dob = new Property
                            {
                                Name = p.Key,
                                Type = p.Value.Type
                            };
                            o.Properties.Add(dob);
                        }
                    }
                    Objects.Add(o);
                }
            }
        }
        static void BuildDependencies(ref List<GenObject> Objects, IDictionary<string, OpenApiSchema> Schemas)
        {
            foreach(KeyValuePair<string, OpenApiSchema> s in Schemas)
            {
                if (s.Value.Reference != null)
                {
                    GenObject o = new GenObject
                    {
                        Name = s.Value.Reference.ReferenceV3,
                        KeyName = s.Key,
                        DependsOn = new List<GenObject>()
                    };
                   
                    foreach(KeyValuePair<string, OpenApiSchema> p in s.Value.Properties)
                    {
                        if(p.Value.Type == "object")
                        {
                            GenObject dob = new GenObject
                            {
                                Name = p.Value.Reference.ReferenceV3,
                                KeyName = p.Key,
                                DependsOn = new List<GenObject>()
                            };
                            o.DependsOn.Add(dob);
                            
                            BuildDependencies(ref Objects, p.Value.Properties);

                        }
                    }
                    Objects.Add(o);
                }
            }
        }

        //ObjectName
        //DependsOn: List<ObjectName>
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

            List<ObjectProperty> ob = new List<ObjectProperty>();
            BuildObjects(ref ob, schemas);
            
            if (objectsToGen.Count > 0)
            {

            }
        }
    }
}
