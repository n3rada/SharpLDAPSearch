using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;

namespace SharpLDAPSearch
{
    class Program
    {
        static void Main(string[] args)
        {
            string serverName = null;
            
            // Check for -Server parameter
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("-Server", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < args.Length)
                    {
                        serverName = args[i + 1];
                    }
                    break;
                }
            }
            
            // Create DirectoryEntry with optional server
            DirectoryEntry entry;
            if (!string.IsNullOrEmpty(serverName))
            {
                entry = new DirectoryEntry("LDAP://" + serverName);
            }
            else
            {
                entry = new DirectoryEntry();
            }
            
            DirectorySearcher mySearcher = new DirectorySearcher(entry);
            List<string> searchProperties = new List<string>();
            //get LDAP search filter
            // Skip -Server and its value when processing positional arguments
            List<string> positionalArgs = new List<string>();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("-Server", StringComparison.OrdinalIgnoreCase))
                {
                    i++; // Skip the server value too
                    continue;
                }
                positionalArgs.Add(args[i]);
            }
            
            if (positionalArgs.Count > 0)
            {
                //example LDAP filter
                //mySearcher.Filter = ("(&(objectCategory=computer)(!(userAccountControl:1.2.840.113556.1.4.803:=2)))");
                mySearcher.Filter = positionalArgs[0];
                if (positionalArgs.Count > 1)
                {
                    searchProperties = positionalArgs[1].Split(',').ToList();
                    foreach (string myKey in searchProperties)
                    {
                        mySearcher.PropertiesToLoad.Add(myKey);
                    }
                }
            }
            else
            {
                Console.WriteLine("[!] No LDAP search provided");
                Console.WriteLine("Usage: SharpLDAPSearch.exe \"(LDAP filter)\" [\"property1,property2,...\"] [-Server servername]");
                return;
            }

            mySearcher.SizeLimit = int.MaxValue;
            mySearcher.PageSize = int.MaxValue;

            try
            {
                foreach (SearchResult mySearchResult in mySearcher.FindAll())
                {
                    // Get the 'DirectoryEntry' that corresponds to 'mySearchResult'.  
                    DirectoryEntry myDirectoryEntry = mySearchResult.GetDirectoryEntry();

                    // Get the properties of the 'mySearchResult'.  
                    ResultPropertyCollection myResultPropColl;
                    myResultPropColl = mySearchResult.Properties;

                    // return only specified attributes
                    if (searchProperties.Count > 0)
                    {
                        foreach (string attr in searchProperties)
                        {
                            // some attributes - such as memberof - have multiple values
                            for (int i = 0; i < mySearchResult.Properties[attr].Count; i++)
                            {
                                Console.WriteLine(mySearchResult.Properties[attr][i].ToString());
                            }
                        }
                    }
                    // if no attributes specified, return all
                    else
                    {
                        foreach (string myKey in myResultPropColl.PropertyNames)
                        {
                            foreach (Object myCollection in myResultPropColl[myKey])
                            {
                                Console.WriteLine("{0} - {1}", myKey, myCollection);
                            }
                        }
                    }

                    mySearcher.Dispose();
                    entry.Dispose(); ;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[!] LDAP Search Error: {0}", ex.Message.Trim());
            }
        }
    }
}
