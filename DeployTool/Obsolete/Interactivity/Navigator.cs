using System;
using System.Collections.Generic;
using System.Text;

namespace DeployTool.Interactivity
{
    public class Navigator
    {
        Dictionary<ConsoleKey, MenuItem> _current;

        Dictionary<ConsoleKey, MenuItem> _baseMenu = new Dictionary<ConsoleKey, MenuItem>();
        Dictionary<ConsoleKey, MenuItem> _invocations1 = new Dictionary<ConsoleKey, MenuItem>();
        Dictionary<ConsoleKey, MenuItem> _invocations2 = new Dictionary<ConsoleKey, MenuItem>();

        void SetupMenus()
        {
            _baseMenu.Add(ConsoleKey.D1, new MenuItem()
            {
                Description = "Menu 1",
                InvokerKind = InvokerKind.InternalChangeMenu,
                InternalInvocationParameter = _invocations1,
            });

            _baseMenu.Add(ConsoleKey.D2, new MenuItem()
            {
                Description = "Menu 2",
                InvokerKind = InvokerKind.InternalChangeMenu,
                InternalInvocationParameter = _invocations2,
            });


            _invocations1.Add(ConsoleKey.D1, new MenuItem()
            {
                Description = "xxx",
                ExternalInvoker = Invoker,
            });

            _invocations1.Add(ConsoleKey.M, new MenuItem()
            {
                Description = "Main Menu",
                InvokerKind = InvokerKind.InternalChangeMenu,
                InternalInvocationParameter = _invocations2,
            });



            _invocations2.Add(ConsoleKey.M, new MenuItem()
            {
                Description = "Main Menu",
                InvokerKind = InvokerKind.InternalChangeMenu,
                InternalInvocationParameter = _invocations2,
            });


        }

        public int RunLoop(string heading, Dictionary<ConsoleKey, MenuItem> startMenu)
        {
            //SetupMenus();

            Console.BackgroundColor = ConsoleColor.DarkRed;
            ConsoleKey key;
            _current = startMenu;

            do
            {
                Clear(heading);
                MenuItem currentChoice;
                var keyInfo = Console.ReadKey(true);
                key = keyInfo.Key;
                if (!_current.TryGetValue(key, out currentChoice))
                    continue;
                switch (currentChoice.InvokerKind)
                {
                    case InvokerKind.External:
                        {
                            var message = currentChoice.ExternalInvoker(keyInfo, currentChoice);
                            if (message == null)
                                continue;
                            PrintMessageAndWait(message);
                        }
                        break;

                    case InvokerKind.InternalChangeMenu:
                        {
                            var nextMenu = currentChoice.InternalInvocationParameter as Dictionary<ConsoleKey, MenuItem>;
                            if (nextMenu == null)
                                continue;
                            _current = nextMenu;
                            PrintMenu();
                        }
                        break;
                }

            }
            while (key != ConsoleKey.Q);

            return 0;
        }

        private void PrintMessageAndWait(string message)
        {
            Console.Clear();
            Console.WriteLine(message);
            Console.WriteLine("");
            Console.WriteLine("Any key to menu");
            Console.ReadKey();
        }


        private void Clear(string heading)
        {
            Console.Clear();
            PrintHeading(heading);
            PrintMenu();
        }

        private void PrintHeading(string heading)
        {
            Console.WriteLine(heading);
            Console.WriteLine("");
        }

        private void PrintMenu()
        {
            foreach (var kp in _current)
            {
                var key = Normalize(kp.Key);
                Console.WriteLine("{0}. {1}", key, kp.Value);
            }
        }

        private string Normalize(ConsoleKey consoleKey)
        {
            var str = consoleKey.ToString().Trim('D');
            return str;
        }



        //string _baseAddress = ConfigurationManager.AppSettings["baseAddress"];

        private string Invoker(ConsoleKeyInfo key, MenuItem choice)
        {
            return "";
            //var address = _baseAddress + choice.RelativeUrl;
            //using (HttpClient client = new HttpClient())
            //{
            //    var request = new HttpRequestMessage(new HttpMethod(choice.Verb.ToUpper()), address);
            //    if (request.Method != HttpMethod.Get)
            //    {

            //        if (choice.Body == null)
            //            choice.Body = string.Empty;
            //        request.Content = new StringContent(choice.Body);
            //    }

            //    var response = client.SendAsync(request).Result;
            //    string responseText = string.Empty;
            //    if (response.Content != null)
            //        responseText = response.Content.ReadAsStringAsync().Result;


            //    //string result;
            //    //if (choice.Verb == "GET")
            //    //    result = client.GetAsync(address).Result.Content.ReadAsStringAsync().Result;
            //    //else if (choice.Verb == "POST")
            //    //    result = client.PostAsync(address, new ByteArrayContent(new byte[] { })).Result.Content.ReadAsStringAsync().Result;
            //    //else
            //    //    throw new NotImplementedException();

            //    var sb = new StringBuilder();
            //    sb.AppendFormat("Request {0}:\r\n", response.IsSuccessStatusCode ? "ok" : "FAILED!");
            //    sb.AppendFormat("{0} {1}\r\n", choice.Verb, choice.RelativeUrl);
            //    sb.AppendLine("Request Body:");
            //    sb.AppendLine(choice.Body);
            //    sb.AppendLine("");
            //    sb.AppendLine("Response headers:");
            //    sb.AppendLine(response.ToString());
            //    sb.AppendLine("");
            //    sb.AppendLine("Response content:");
            //    sb.AppendLine(responseText);
            //    return sb.ToString();
            //}
        }

        //Dictionary<string, IDisposable> _proxies = new Dictionary<string, IDisposable>();
        private string SignalRInvoker(ConsoleKeyInfo key, MenuItem choice)
        {
            return "";
            //if (choice.Parameters == null)
            //    return string.Empty;
            //var parameters = choice.Parameters.Split(' ');
            //if (parameters.Length < 5)
            //    return string.Empty;

            //var hubName = parameters[1];
            //var adapterName = parameters[2];
            //var methodCall = parameters[3];
            //var callback = parameters[4];
            //var filter = choice.Body;

            //IDisposable disposable;
            //if (_proxies.TryGetValue(hubName, out disposable))
            //    disposable.Dispose();

            //var hubConnection = new HubConnection(_baseAddress);
            //hubConnection.Headers.Add("SimaticIt.Name", "TestClient-" + _uniqueClientName.ToString());
            ////hubConnection.TraceLevel = TraceLevels.All;
            ////hubConnection.TraceWriter = Console.Out;

            //var serverHub = hubConnection.CreateHubProxy(hubName);
            //hubConnection.TransportConnectTimeout = TimeSpan.FromSeconds(10);
            //hubConnection.Reconnecting += () => Console.WriteLine("Reconnecting ...");
            //hubConnection.Reconnected += () => Console.WriteLine("Reconnected!!!");
            //hubConnection.ConnectionSlow += () => Console.WriteLine("Connection is slow");
            //hubConnection.Closed += () => Console.WriteLine("Connection was closed");
            //hubConnection.Error += err => Console.WriteLine("Error: {0}", err.ToString());
            //hubConnection.StateChanged += state => Console.WriteLine("State Changed: {0} => {1}", state.OldState, state.NewState);
            //_proxies[hubName] = serverHub.On<object>(callback, message =>
            //    {
            //        dynamic msg = message;
            //        string id = msg.IdStation;
            //        //if (id == "LWP0199")
            //        System.Console.WriteLine(message);
            //    });
            //serverHub.On("requestRegister", () => Console.WriteLine("registered!"));
            //hubConnection.Start().Wait();

            //serverHub.Invoke(methodCall, filter, adapterName).Wait();

            ////string line = null;
            ////while ((line = System.Console.ReadLine()) != null)
            ////{
            ////    if (line == string.Empty)
            ////        continue;
            ////    serverHub.Invoke(methodCall, line, adapterName).Wait();
            ////}

            //var sb = new StringBuilder();
            //sb.AppendFormat("SignalR wait on: {0} - Filter:\r\n", choice.Parameters, filter);
            //sb.AppendLine("");
            //return sb.ToString();
        }




    }
}
