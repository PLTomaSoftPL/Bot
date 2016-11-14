namespace CarouselCardsBot
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using System.Net;
    using System.IO;
    using System.Text;
    using System.Linq;
    using HtmlAgilityPack;

    [Serializable]
    public class CarouselCardsDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            //            //    var reply = context.MakeMessage();

            //            //    reply.AttachmentLayout = AttachmentLayoutTypes.List;
            //            //    reply.Attachments = GetCardsAttachments();

            //            var reply = context.MakeMessage();
            //            reply.Attachments = new List<Attachment>();

            //            var signinCard = new SigninCard()
            //            {
            //                Text = "Wybierz pozycje zamówienia",
            //                Buttons = new List<CardAction> {
            //     new CardAction()
            //        {
            //            Value = "Pizza",
            //            Type = "postBack",
            //            Title = "Pizza",

            //          //  Image = "https://cdn1.iconfinder.com/data/icons/logotypes/32/square-facebook-128.png"
            //        },
            //     new CardAction()
            //        {
            //            Value = "Makaron",
            //            Type = "postBack",
            //            Title = "Makaron",
            //         //   Image = "http://images.dailytech.com/nimage/G_is_For_Google_New_Logo_Thumb.png"
            //        }
            //    }
            //            };

            //            reply.Attachments = new List<Attachment> {
            //    signinCard.ToAttachment()
            //};

            //            await context.PostAsync(reply);

            //            context.Wait(this.MessageReceivedAsync);




            var reply = context.MakeMessage();

            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = GetCardsAttachments();

            await context.PostAsync(reply);

            context.Wait(this.MessageReceivedAsync);


        }

        private static IList<Attachment> GetCardsAttachments()
        {
            List<Attachment> list = new List<Attachment>();



            string urlAddress = "http://www.plusliga.pl";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "owl-carousel-home-slider-2";
                string xpath = String.Format("//div[@id='{0}']/div", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person;
                }



                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                var hrefList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found")).Where(p =>p.Contains("/news/")).GroupBy(p =>p.ToString())
                                  .ToList();
                
                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("alt", "not found"))
                                  .ToList();

                response.Close();
                readStream.Close();


                for (int i = 0; i < 3; i++)
                {
                    list.Add(GetHeroCard(
                           titleList[i], "", "",
                           new CardImage(url: imgList[i]),
                           new CardAction(ActionTypes.OpenUrl, "Czytaj więcej", value: "http://plusliga.pl" + hrefList[i].Key))
                       );
                }
            }
            return list;
            //return new List<Attachment>()
            //{
            //    for(int i=0;i<5;i++)
            //    GetHeroCard(
            //        "Azure Storage",
            //        "Massively scalable cloud storage for your applications",
            //        "Store and help protect your data. Get durable, highly available data storage across the globe and pay only for what you use.",
            //        new CardImage(url: "https://acom.azurecomcdn.net/80C57D/cdn/mediahandler/docarticles/dpsmedia-prod/azure.microsoft.com/en-us/documentation/articles/storage-introduction/20160801042915/storage-concepts.png"),
            //        new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/storage/")),
            //  //  GetThumbnailCard(
            //    //    "DocumentDB",
            //    //    "Blazing fast, planet-scale NoSQL",
            //    //    "NoSQL service for highly available, globally distributed apps—take full advantage of SQL and JavaScript over document and key-value data without the hassles of on-premises or virtual machine-based cloud database options.",
            //    //    new CardImage(url: "https://sec.ch9.ms/ch9/29f4/beb4b953-ab91-4a31-b16a-71fb6d6829f4/WhatisAzureDocumentDB_960.jpg"),
            //    //    new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/documentdb/")),
            //    //GetHeroCard(
            //    //    "Azure Functions",
            //    //    "Process events with serverless code",
            //    //    "Azure Functions is a serverless event driven experience that extends the existing Azure App Service platform. These nano-services can scale based on demand and you pay only for the resources you consume.",
            //    //    new CardImage(url: "https://azurecomcdn.azureedge.net/cvt-8636d9bb8d979834d655a5d39d1b4e86b12956a2bcfdb8beb04730b6daac1b86/images/page/services/functions/azure-functions-screenshot.png"),
            //    //    new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/functions/")),
            //    //GetThumbnailCard(
            //    //    "Cognitive Services",
            //    //    "Build powerful intelligence into your applications to enable natural and contextual interactions",
            //    //    "Enable natural and contextual interaction with tools that augment users' experiences using the power of machine-based intelligence. Tap into an ever-growing collection of powerful artificial intelligence algorithms for vision, speech, language, and knowledge.",
            //    //    new CardImage(url: "https://azurecomcdn.azureedge.net/cvt-8636d9bb8d979834d655a5d39d1b4e86b12956a2bcfdb8beb04730b6daac1b86/images/page/services/functions/azure-functions-screenshot.png"),
            //    //    new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/functions/")),

            //};
        }

        private static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Buttons = new List<CardAction>() { cardAction },
            };

            return heroCard.ToAttachment();
        }

        private static Attachment GetThumbnailCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Buttons = new List<CardAction>() { cardAction },
            };

            return heroCard.ToAttachment();
        }
        private static Attachment GetCardsAttachments(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
        {
            var heroCard = new ThumbnailCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Buttons = new List<CardAction>() { cardAction },
            };

            return heroCard.ToAttachment();
        }
    }
//    public class DOdatkiDoPizzy : IDialog<object>
//    {
//        public async Task StartAsync(IDialogContext context)
//        {
//            context.Wait(this.MessageReceivedAsync2);
//        }

//        public virtual async Task MessageReceivedAsync2(IDialogContext context, IAwaitable<IMessageActivity> result)
//        {
//            //    var reply = context.MakeMessage();

//            //    reply.AttachmentLayout = AttachmentLayoutTypes.List;
//            //    reply.Attachments = GetCardsAttachments();

//            var reply = context.MakeMessage();
//            reply.Attachments = new List<Attachment>();

//            var signinCard = new SigninCard()
//            {
//                Text = "Jakie dodatki do pizzy?",
//                Buttons = new List<CardAction> {
//     new CardAction()
//        {
//            Value = "Dodatek 1",
//            Type = "postBack",
//            Title = "Dodatek 1",
            
//          //  Image = "https://cdn1.iconfinder.com/data/icons/logotypes/32/square-facebook-128.png"
//        },
//  new CardAction()
//        {
//            Value = "Dodatek 2",
//            Type = "postBack",
//            Title = "Dodatek 2",
            
//          //  Image = "https://cdn1.iconfinder.com/data/icons/logotypes/32/square-facebook-128.png"
//        },
//    new CardAction()
//        {
//            Value = "Dodatek 3",
//            Type = "postBack",
//            Title = "Dodatek 3",
            
//          //  Image = "https://cdn1.iconfinder.com/data/icons/logotypes/32/square-facebook-128.png"
//        },
//      new CardAction()
//        {
//            Value = "Wystarczy",
//            Type = "postBack",
//            Title = "Wystarczy",
            
//          //  Image = "https://cdn1.iconfinder.com/data/icons/logotypes/32/square-facebook-128.png"
//        },
//    }
//            };

//            reply.Attachments = new List<Attachment> {
//    signinCard.ToAttachment()
//};

//            await context.PostAsync(reply);

//            context.Wait(this.MessageReceivedAsync2);
//        }

//        private static IList<Attachment> GetCardsAttachments()
//        {
//            return new List<Attachment>()
//            {
//                GetHeroCard(
//                    "Azure Storage",
//                    "Massively scalable cloud storage for your applications",
//                    "Store and help protect your data. Get durable, highly available data storage across the globe and pay only for what you use.",
//                    new CardImage(url: "https://acom.azurecomcdn.net/80C57D/cdn/mediahandler/docarticles/dpsmedia-prod/azure.microsoft.com/en-us/documentation/articles/storage-introduction/20160801042915/storage-concepts.png"),
//                    new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/storage/")),
//              //  GetThumbnailCard(
//                //    "DocumentDB",
//                //    "Blazing fast, planet-scale NoSQL",
//                //    "NoSQL service for highly available, globally distributed apps—take full advantage of SQL and JavaScript over document and key-value data without the hassles of on-premises or virtual machine-based cloud database options.",
//                //    new CardImage(url: "https://sec.ch9.ms/ch9/29f4/beb4b953-ab91-4a31-b16a-71fb6d6829f4/WhatisAzureDocumentDB_960.jpg"),
//                //    new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/documentdb/")),
//                //GetHeroCard(
//                //    "Azure Functions",
//                //    "Process events with serverless code",
//                //    "Azure Functions is a serverless event driven experience that extends the existing Azure App Service platform. These nano-services can scale based on demand and you pay only for the resources you consume.",
//                //    new CardImage(url: "https://azurecomcdn.azureedge.net/cvt-8636d9bb8d979834d655a5d39d1b4e86b12956a2bcfdb8beb04730b6daac1b86/images/page/services/functions/azure-functions-screenshot.png"),
//                //    new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/functions/")),
//                //GetThumbnailCard(
//                //    "Cognitive Services",
//                //    "Build powerful intelligence into your applications to enable natural and contextual interactions",
//                //    "Enable natural and contextual interaction with tools that augment users' experiences using the power of machine-based intelligence. Tap into an ever-growing collection of powerful artificial intelligence algorithms for vision, speech, language, and knowledge.",
//                //    new CardImage(url: "https://azurecomcdn.azureedge.net/cvt-8636d9bb8d979834d655a5d39d1b4e86b12956a2bcfdb8beb04730b6daac1b86/images/page/services/functions/azure-functions-screenshot.png"),
//                //    new CardAction(ActionTypes.OpenUrl, "Learn more", value: "https://azure.microsoft.com/en-us/services/functions/")),

//            };
//        }

//        private static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
//        {
//            var heroCard = new HeroCard
//            {
//                Title = "asd",
//                Subtitle = "ads",
//                Text = "asd",
//                //      Images = new List<CardImage>() { cardImage },
//                Buttons = new List<CardAction>() { cardAction },
//            };

//            return heroCard.ToAttachment();
//        }

//        private static Attachment GetThumbnailCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
//        {
//            var heroCard = new HeroCard
//            {
//                Title = title,
//                Subtitle = subtitle,
//                Text = text,
//                Images = new List<CardImage>() { cardImage },
//                Buttons = new List<CardAction>() { cardAction },
//            };

//            return heroCard.ToAttachment();
//        }
//        private static Attachment GetCardsAttachments(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
//        {
//            var heroCard = new ThumbnailCard
//            {
//                Title = title,
//                Subtitle = subtitle,
//                Text = text,
//                Images = new List<CardImage>() { cardImage },
//                Buttons = new List<CardAction>() { cardAction },
//            };

//            return heroCard.ToAttachment();
//        }
//    }
}