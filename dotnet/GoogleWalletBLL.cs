/*
 * Copyright 2022 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

// [START setup]
// [START imports]
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Walletobjects.v1;
using Google.Apis.Walletobjects.v1.Data;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Models.EventTicketDTOS;
// [END imports]

namespace GoogleWallet.BLL 
{

    /// <summary>
    /// Demo class for creating and managing Generic passes in Google Wallet.
    /// </summary>
    public class GoogleWalletBLL : GoogleWalletBLLBase, IGoogleWalletBLL
    {

        // [START createClass]
        /// <summary>
        /// Create a class.
        /// </summary>
        /// <param name="issuerId">The issuer ID being used for this request.</param>
        /// <param name="classSuffix">Developer-defined unique ID for this pass class.</param>
        /// <returns>The pass class ID: "{issuerId}.{classSuffix}"</returns>
        public string CreateClass(string issuerId, string classSuffix)
        {
            // Check if the class exists
            Stream responseStream = service.Genericclass
                .Get($"{issuerId}.{classSuffix}")
                .ExecuteAsStream();

            StreamReader responseReader = new StreamReader(responseStream);
            JObject jsonResponse = JObject.Parse(responseReader.ReadToEnd());

            if (!jsonResponse.ContainsKey("error"))
            {
                Console.WriteLine($"Class {issuerId}.{classSuffix} already exists!");
                return $"{issuerId}.{classSuffix}";
            }
            else if (jsonResponse["error"].Value<int>("code") != 404)
            {
                // Something else went wrong...
                Console.WriteLine(jsonResponse.ToString());
                return $"{issuerId}.{classSuffix}";
            }

            // See link below for more information on required properties
            // https://developers.google.com/wallet/generic/rest/v1/genericclass
            GenericClass newClass = new GenericClass
            {
                Id = $"{issuerId}.{classSuffix}"
                
            };

            responseStream = service.Genericclass
                .Insert(newClass)
                .ExecuteAsStream();

            responseReader = new StreamReader(responseStream);
            jsonResponse = JObject.Parse(responseReader.ReadToEnd());

            Console.WriteLine("Class insert response");
            Console.WriteLine(jsonResponse.ToString());

            return $"{issuerId}.{classSuffix}";
        }
        // [END createClass]

        // [START updateClass]
        /// <summary>
        /// Update a class.
        /// <para />
        /// <strong>Warning:</strong> This replaces all existing class attributes!
        /// </summary>
        /// <param name="issuerId">The issuer ID being used for this request.</param>
        /// <param name="classSuffix">Developer-defined unique ID for this pass class.</param>
        /// <returns>The pass class ID: "{issuerId}.{classSuffix}"</returns>
        //public string UpdateClass(string issuerId, string classSuffix)
        //{
        //    // Check if the class exists
        //    Stream responseStream = service.Genericclass
        //        .Get($"{issuerId}.{classSuffix}")
        //        .ExecuteAsStream();

        //    StreamReader responseReader = new StreamReader(responseStream);
        //    JObject jsonResponse = JObject.Parse(responseReader.ReadToEnd());

        //    if (jsonResponse.ContainsKey("error"))
        //    {
        //        if (jsonResponse["error"].Value<int>("code") == 404)
        //        {
        //            // Class does not exist
        //            Console.WriteLine($"Class {issuerId}.{classSuffix} not found!");
        //            return $"{issuerId}.{classSuffix}";
        //        }
        //        else
        //        {
        //            // Something else went wrong...
        //            Console.WriteLine(jsonResponse.ToString());
        //            return $"{issuerId}.{classSuffix}";
        //        }
        //    }

        //    // Class exists
        //    GenericClass updatedClass = JsonConvert.DeserializeObject<GenericClass>(jsonResponse.ToString());

        //    // Update the class by adding a link
        //    Google.Apis.Walletobjects.v1.Data.Uri newLink = new Google.Apis.Walletobjects.v1.Data.Uri
        //    {
        //        UriValue = "https://developers.google.com/wallet",
        //        Description = "New link description"
        //    };

        //    if (updatedClass.LinksModuleData == null)
        //    {
        //        // LinksModuleData was not set on the original object
        //        updatedClass.LinksModuleData = new LinksModuleData
        //        {
        //            Uris = new List<Google.Apis.Walletobjects.v1.Data.Uri>
        //{
        //  newLink
        //}
        //        };
        //    }
        //    else
        //    {
        //        // LinksModuleData was set on the original object
        //        updatedClass.LinksModuleData.Uris.Add(newLink);
        //    }

        //    responseStream = service.Genericclass
        //        .Update(updatedClass, $"{issuerId}.{classSuffix}")
        //        .ExecuteAsStream();

        //    responseReader = new StreamReader(responseStream);
        //    jsonResponse = JObject.Parse(responseReader.ReadToEnd());

        //    Console.WriteLine("Class update response");
        //    Console.WriteLine(jsonResponse.ToString());

        //    return $"{issuerId}.{classSuffix}";
        //}
        // [END updateClass]

        // [START patchClass]
        /// <summary>
        /// Patch a class.
        /// <para />
        /// The PATCH method supports patch semantics.
        /// </summary>
        /// <param name="issuerId">The issuer ID being used for this request.</param>
        /// <param name="classSuffix">Developer-defined unique ID for this pass class.</param>
        /// <returns>The pass class ID: "{issuerId}.{classSuffix}"</returns>




        // [START jwtNew]
        /// <summary>
        /// Generate a signed JWT that creates a new pass class and object.
        /// <para />
        /// When the user opens the "Add to Google Wallet" URL and saves the pass to
        /// their wallet, the pass class and object defined in the JWT are created.
        /// This allows you to create multiple pass classes and objects in one API
        /// call when the user saves the pass to their wallet.
        /// <para />
        /// The Google Wallet C# library uses Newtonsoft.Json.JsonPropertyAttribute
        /// to specify the property names when converting objects to JSON. The
        /// Newtonsoft.Json.JsonConvert.SerializeObject method will automatically
        /// serialize the object with the right property names.
        /// </summary>
        /// <param name="issuerId">The issuer ID being used for this request.</param>
        /// <param name="classSuffix">Developer-defined unique ID for this pass class.</param>
        /// <param name="objectSuffix">Developer-defined unique ID for the pass object.</param>
        /// <returns>An "Add to Google Wallet" link.</returns>
        public async Task<string> CreateJWTNewObjects(string objectSuffix, EventTicketDTO eventTicketDTO)
        {
            try
            {
                string issuerId = "3388000000022248227";
                string classSuffix = "TesteTeste2";

                var res = CreateClass(issuerId, classSuffix);

                // Ignore null values when serializing to/from JSON
                JsonSerializerSettings excludeNulls = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                };

                // See link below for more information on required properties
                // https://developers.google.com/wallet/generic/rest/v1/genericclass

                // See link below for more information on required properties
                // https://developers.google.com/wallet/generic/rest/v1/genericobject

                
                GenericObject newObject = new GenericObject
                {
                    Id = $"{issuerId}.{objectSuffix}",
                    ClassId = $"{issuerId}.{classSuffix}",
                    State = "ACTIVE",
                    Barcode = new Barcode
                    {
                        Type = "QR_CODE",
                        Value = eventTicketDTO.QRCode
                    },
                    CardTitle = new LocalizedString
                    {
                        DefaultValue = new TranslatedString
                        {
                            Language = "en-US",
                            Value = eventTicketDTO.CardTitleHeader
                        }
                    },
                    Header = new LocalizedString
                    {
                        DefaultValue = new TranslatedString
                        {
                            Language = "en-US",
                            Value = eventTicketDTO.CardTitle
                        }
                    },
                    Subheader = new LocalizedString
                    {
                        DefaultValue = new TranslatedString
                        {
                            Language = "en-US",
                            Value = eventTicketDTO.Date
                        }
                    },
                    HexBackgroundColor = eventTicketDTO.PassBackgroundColor,
                    Logo = new Image
                    {
                        SourceUri = new ImageUri
                        {
                            Uri = "https://res.cloudinary.com/crunchbase-production/image/upload/c_lpad,h_256,w_256,f_auto,q_auto:eco,dpr_1/v1496433508/yrux4wyqff9sykh27nx2.jpg"
                        },
                        ContentDescription = new LocalizedString
                        {
                            DefaultValue = new TranslatedString
                            {
                                Language = "en-US",
                                Value = "Benfica"
                            }
                        },
                    },//Nicholas Alteen // here i define textmodeledata
                    TextModulesData = new List<TextModuleData>
                    {
                        new TextModuleData
                        {
                        Header = eventTicketDTO.NameLabel,
                        Body = eventTicketDTO.Name,
                        Id = "nome"


                        },
                        new TextModuleData
                        {
                        Header = eventTicketDTO.AssociatedLabel,
                        Body = eventTicketDTO.Associated,
                        Id = "socio"
                        },

                        new TextModuleData
                        {
                        Header = eventTicketDTO.BenchLabel,
                        Body = eventTicketDTO.Bench,
                        Id = "bancada",

                        },
                        new TextModuleData
                        {
                        Header = eventTicketDTO.GateLabel,
                        Body = eventTicketDTO.Gate,
                        Id = "porta"
                        },
                        new TextModuleData
                        {
                        Header = eventTicketDTO.SectionLabel,
                        Body = eventTicketDTO.Section,
                        Id = "sector"
                        },new TextModuleData
                        {
                        Header = eventTicketDTO.FloorLabel,
                        Body = eventTicketDTO.Floor,
                        Id = "piso"
                        },
                        new TextModuleData
                        {
                        Header = eventTicketDTO.RowLabel,
                        Body = eventTicketDTO.Row,
                        Id = "fila"
                        },
                        new TextModuleData
                        {
                        Header = eventTicketDTO.SeatLabel,
                        Body = eventTicketDTO.Seat,
                        Id = "lugar"
                        }
                    },
                    
              
                };

                GenericClass newClass = new GenericClass
                {
                    Id = $"{issuerId}.{classSuffix}",
                    ClassTemplateInfo = new ClassTemplateInfo  //Nicholas Alteen // here i define template to use on texmouledata
                    {
                        CardTemplateOverride = new CardTemplateOverride
                        {
                            CardRowTemplateInfos = new List<CardRowTemplateInfo>
                            {
                                new CardRowTemplateInfo()
                                {
                                    OneItem = new CardRowOneItem
                                    {
                                       Item = new TemplateItem
                                       {
                                            FirstValue= new FieldSelector
                                            {
                                                Fields = new List<FieldReference>
                                                {
                                                    new FieldReference
                                                    {
                                                        FieldPath = "object.payload.genericObjects[0].textModulesData[0].id" //here is my doubts of how i should reference

                                                    }
                                                }
                                            },
                                            SecondValue= new FieldSelector
                                            {
                                                Fields = new List<FieldReference>
                                                {
                                                    new FieldReference
                                                    {
                                                        FieldPath = "object.payload.genericObjects[0].textModulesData[0].id"
                                                    }
                                                }
                                            }


                                       }
                                    }
                                },

                                new CardRowTemplateInfo()
                                {
                                    ThreeItems = new CardRowThreeItems
                                    {
                                        StartItem = new TemplateItem
                                        {
                                            FirstValue= new FieldSelector
                                            {
                                                Fields = new List<FieldReference>
                                                {
                                                    new FieldReference
                                                    {
                                                        FieldPath = "object.payload.genericObjects[0].textModulesData[0].id"

                                                    }

                                                }
                                            }

                                        },
                                        MiddleItem = new TemplateItem
                                        {
                                            FirstValue= new FieldSelector
                                            {
                                                Fields = new List<FieldReference>
                                                {
                                                    new FieldReference
                                                    {
                                                        FieldPath = "object.payload.genericObjects[0].textModulesData[0].id"


                                                    }
                                                }
                                            }
                                        },
                                        EndItem = new TemplateItem
                                        {
                                            FirstValue= new FieldSelector
                                            {
                                                Fields = new List<FieldReference>
                                                {
                                                    new FieldReference
                                                    {
                                                        FieldPath = "object.payload.genericObjects[0].textModulesData[0].id"
                                                    }
                                                }
                                            }
                                        },

                                    }
                                },
                                new CardRowTemplateInfo()
                                {
                                    ThreeItems = new CardRowThreeItems
                                    {
                                        StartItem = new TemplateItem
                                        {
                                            FirstValue= new FieldSelector
                                            {
                                                Fields = new List<FieldReference>
                                                {
                                                    new FieldReference
                                                    {
                                                        FieldPath = "object.payload.genericObjects[0].textModulesData[0].id"
                                                    }
                                                }
                                            }
                                        },
                                        MiddleItem = new TemplateItem
                                        {
                                            FirstValue= new FieldSelector
                                            {
                                                Fields = new List<FieldReference>
                                                {
                                                    new FieldReference
                                                    {
                                                        FieldPath = "object.textModulesData['fila']"


                                                    }
                                                }
                                            }
                                        },
                                        EndItem = new TemplateItem
                                        {
                                            FirstValue= new FieldSelector
                                            {
                                                Fields = new List<FieldReference>
                                                {
                                                    new FieldReference
                                                    {
                                                        FieldPath = "object.textModulesData['lugar']"
                                                    }
                                                }
                                            }
                                        },

                                    }
                                }
                            }
                        }
                    }


                };

                // Create JSON representations of the class and object
                //JObject serializedClass = JObject.Parse(
                //    JsonConvert.SerializeObject(newClass, excludeNulls));
                JObject serializedObject = JObject.Parse(
                    JsonConvert.SerializeObject(newObject, excludeNulls));

                // Create the JWT as a JSON object
                JObject jwtPayload = JObject.Parse(JsonConvert.SerializeObject(new
                {
                    iss = credentials.Id,
                    aud = "google",
                    origins = new List<string>
                    {
                    "www.example.com"
                    },
                    typ = "savetowallet",
                    payload = JObject.Parse(JsonConvert.SerializeObject(new
                    {
                        // The listed classes and objects will be created
                        // when the user saves the pass to their wallet
                    //    genericClasses = new List<JObject>
                    //{
                    //  serializedClass
                    //},
                        genericObjects = new List<JObject>
                    {
                      serializedObject
                    }
                    }))
                }));

                // Deserialize into a JwtPayload
                JwtPayload claims = JwtPayload.Deserialize(jwtPayload.ToString());

                // The service account credentials are used to sign the JWT
                RsaSecurityKey key = new RsaSecurityKey(credentials.Key);
                SigningCredentials signingCredentials = new SigningCredentials(
                    key, SecurityAlgorithms.RsaSha256);
                JwtSecurityToken jwt = new JwtSecurityToken(
                    new JwtHeader(signingCredentials), claims);
                string token = new JwtSecurityTokenHandler().WriteToken(jwt);

                Console.WriteLine("Add to Google Wallet link");
                Console.WriteLine($"https://pay.google.com/gp/v/save/{token}");

                return await Task.FromResult($"https://pay.google.com/gp/v/save/{token}");
            }
            catch(Exception e)
            { 
                Console.WriteLine(e.ToString());
                throw e;
            }
        }



        // [END jwtNew]

        // [START jwtExisting]
        /// <summary>
        /// Generate a signed JWT that references an existing pass object.
        /// <para />
        /// When the user opens the "Add to Google Wallet" URL and saves the pass to
        /// their wallet, the pass objects defined in the JWT are added to the user's
        /// Google Wallet app. This allows the user to save multiple pass objects in
        /// one API call.
        /// <para />
        /// The objects to add must follow the below format:
        /// <para />
        /// { 'id': 'ISSUER_ID.OBJECT_SUFFIX', 'classId': 'ISSUER_ID.CLASS_SUFFIX' }
        /// <para />
        /// The Google Wallet C# library uses Newtonsoft.Json.JsonPropertyAttribute
        /// to specify the property names when converting objects to JSON. The
        /// Newtonsoft.Json.JsonConvert.SerializeObject method will automatically
        /// serialize the object with the right property names.
        /// </summary>
        /// <param name="issuerId">The issuer ID being used for this request.</param>
        /// <returns>An "Add to Google Wallet" link.</returns>
        public string CreateJWTExistingObjects(string issuerId)
        {
            // Ignore null values when serializing to/from JSON
            JsonSerializerSettings excludeNulls = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            // Multiple pass types can be added at the same time
            // At least one type must be specified in the JWT claims
            // Note: Make sure to replace the placeholder class and object suffixes
            Dictionary<string, Object> objectsToAdd = new Dictionary<string, Object>();

            // Event tickets
            objectsToAdd.Add("eventTicketObjects", new List<EventTicketObject>
    {
      new EventTicketObject
      {
        Id = $"{issuerId}.EVENT_OBJECT_SUFFIX",
        ClassId = $"{issuerId}.EVENT_CLASS_SUFFIX"
      }
    });

            // Boarding passes
            objectsToAdd.Add("flightObjects", new List<FlightObject>
    {
      new FlightObject
      {
        Id = $"{issuerId}.FLIGHT_OBJECT_SUFFIX",
        ClassId = $"{issuerId}.FLIGHT_CLASS_SUFFIX"
      }
    });

            // Generic passes
            objectsToAdd.Add("genericObjects", new List<GenericObject>
    {
      new GenericObject
      {
        Id = $"{issuerId}.GENERIC_OBJECT_SUFFIX",
        ClassId = $"{issuerId}.GENERIC_CLASS_SUFFIX"
      }
    });

            // Gift cards
            objectsToAdd.Add("giftCardObjects", new List<GiftCardObject>
    {
      new GiftCardObject
      {
        Id = $"{issuerId}.GIFT_CARD_OBJECT_SUFFIX",
        ClassId = $"{issuerId}.GIFT_CARD_CLASS_SUFFIX"
      }
    });

            // Loyalty cards
            objectsToAdd.Add("loyaltyObjects", new List<LoyaltyObject>
    {
      new LoyaltyObject
      {
        Id = $"{issuerId}.LOYALTY_OBJECT_SUFFIX",
        ClassId = $"{issuerId}.LOYALTY_CLASS_SUFFIX"
      }
    });

            // Offers
            objectsToAdd.Add("offerObjects", new List<OfferObject>
    {
      new OfferObject
      {
        Id = $"{issuerId}.OFFER_OBJECT_SUFFIX",
        ClassId = $"{issuerId}.OFFER_CLASS_SUFFIX"
      }
    });

            // Transit passes
            objectsToAdd.Add("transitObjects", new List<TransitObject>
    {
      new TransitObject
      {
        Id = $"{issuerId}.TRANSIT_OBJECT_SUFFIX",
        ClassId = $"{issuerId}.TRANSIT_CLASS_SUFFIX"
      }
    });

            // Create a JSON representation of the payload
            JObject serializedPayload = JObject.Parse(
                JsonConvert.SerializeObject(objectsToAdd, excludeNulls));

            // Create the JWT as a JSON object
            JObject jwtPayload = JObject.Parse(JsonConvert.SerializeObject(new
            {
                iss = credentials.Id,
                aud = "google",
                origins = new string[]
              {
        "www.example.com"
              },
                typ = "savetowallet",
                payload = serializedPayload
            }));

            // Deserialize into a JwtPayload
            JwtPayload claims = JwtPayload.Deserialize(jwtPayload.ToString());

            // The service account credentials are used to sign the JWT
            RsaSecurityKey key = new RsaSecurityKey(credentials.Key);
            SigningCredentials signingCredentials = new SigningCredentials(
                key, SecurityAlgorithms.RsaSha256);
            JwtSecurityToken jwt = new JwtSecurityToken(
                new JwtHeader(signingCredentials), claims);
            string token = new JwtSecurityTokenHandler().WriteToken(jwt);

            Console.WriteLine("Add to Google Wallet link");
            Console.WriteLine($"https://pay.google.com/gp/v/save/{token}");

            return $"https://pay.google.com/gp/v/save/{token}";
        }
        // [END jwtExisting]

       
    }
}
