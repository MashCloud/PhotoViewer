using Microsoft.Identity.Client;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Net.Http.Json;

namespace active_directory_wpf_msgraph_v2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        /*
         https://learn.microsoft.com/en-us/answers/questions/891434/unable-to-find-target-address-error-while-using-mi
        https://stackoverflow.com/questions/76098374/how-to-search-for-files-in-onedrive-using-msgraph-query-api
         */
        //

        //Set the API Endpoint to Graph 'me' endpoint. 
        // To change from Microsoft public cloud to a national cloud, use another value of graphAPIEndpoint.
        // Reference with Graph endpoints here: https://docs.microsoft.com/graph/deployments#microsoft-graph-and-graph-explorer-service-root-endpoints
        string graphAPIEndpoint = "https://graph.microsoft.com/v1.0/me";
        string apiUrlMyDrive = "https://graph.microsoft.com/v1.0/me/drive";
        //file: response-img005
        //string apiUrlMyDriveFolder1 = "https://graph.microsoft.com/v1.0/me/drive/search(q='img005')";
        //file: response-img005
        string apiUrlMyDriveFolder1 = "https://graph.microsoft.com/v1.0/me/drive/search(q='model_test')";


        //string apiUrlRootChildren = "https://graph.microsoft.com/v1.0/me/drive/root/children";
        String path1 = "Photos/PhotoViewer/Folder1";
        string apiUrlRootChildren =
            "https://graph.microsoft.com/v1.0/me/drive/root:/Photos/PhotoViewer/Folder1:/children";
        //string apiUrlRootChildren =
        //          "https://graph.microsoft.com/v1.0/me/drive/root:/Photos/PhotoViewer/Folder1/search(q='{IMG_0006}')";


        String postQueryUrl = "https://graph.microsoft.com/v1.0/search/query";



        //Set the scope for API call to user.read
        string[] scopes = new string[] { "user.read","Files.Read","Files.Read.All" };


        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Call AcquireToken - to acquire a token requiring user to sign-in
        /// </summary>
        private async void CallGraphButton_Click(object sender, RoutedEventArgs e)
        {
            AuthenticationResult authResult = null;
            var app = App.PublicClientApp;
            ResultText.Text = string.Empty;
            TokenInfoText.Text = string.Empty;

            // if the user signed-in before, remember the account info from the cache
            IAccount firstAccount = (await app.GetAccountsAsync()).FirstOrDefault();

            // otherwise, try witht the Windows account
            if (firstAccount == null)
            {
                firstAccount = PublicClientApplication.OperatingSystemAccount; 
            }

            try
            {
                authResult = await app.AcquireTokenSilent(scopes, firstAccount)
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilent. 
                // This indicates you need to call AcquireTokenInteractive to acquire a token
                System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                try
                {
                    authResult = await app.AcquireTokenInteractive(scopes)
                        .WithAccount(firstAccount)
                        .WithParentActivityOrWindow(new WindowInteropHelper(this).Handle) // optional, used to center the browser on the window
                        .WithPrompt(Prompt.SelectAccount)
                        .ExecuteAsync();
                }
                catch (MsalException msalex)
                {
                    ResultText.Text = $"Error Acquiring Token:{System.Environment.NewLine}{msalex}";
                }
            }
            catch (Exception ex)
            {
                ResultText.Text = $"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}";
                return;
            }

            if (authResult != null)
            {
                //me
                //  ResultText.Text = await GetHttpContentWithToken(graphAPIEndpoint, authResult.AccessToken);
                //drive
                //ResultText.Text = await GetHttpContentWithToken(apiUrlMyDrive, authResult.AccessToken);
                //search
                File.WriteAllText(@"C:\data\response-token.json", "Bearer "+authResult.AccessToken);

                String s = await PostHttpContentWithToken(postQueryUrl, authResult.AccessToken,new { });
                Console.WriteLine(s);
                ResultText.Text = s;
                File.WriteAllText(@"C:\data\response-post.json", s);

                DisplayBasicTokenInfo(authResult);
                this.SignOutButton.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Perform an HTTP GET request to a URL using an HTTP Authorization header
        /// </summary>
        /// <param name="url">The URL</param>
        /// <param name="token">The token</param>
        /// <returns>String containing the results of the GET operation</returns>
        public async Task<string> GetHttpContentWithToken(string url, string token) {
            var httpClient = new System.Net.Http.HttpClient();
            System.Net.Http.HttpResponseMessage response;
            try
            {
                var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
                //Add the token in Authorization header
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public async Task<string> PostHttpContentWithToken(string url, string token,Object jsonObject) {
            var httpClient = new System.Net.Http.HttpClient();
            System.Net.Http.HttpResponseMessage response;
            try
            {
                var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, 
                    new Uri(url));
                //request.Content = JsonContent.Create(new
                //{
                //    requests= [
                //                      {
                //    entityTypes= [
                //                          "driveItem"
                //                        ],
                //      query= {
                //        queryString= "sun"
                //      }
                //}
                //  ]
                //});
                //request.Content = JsonContent.Create(new { name = "img"});
                //Add the token in Authorization header
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        /// <summary>
        /// Sign out the current user
        /// </summary>
        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            var accounts = await App.PublicClientApp.GetAccountsAsync();
            if (accounts.Any())
            {
                try
                {
                    await App.PublicClientApp.RemoveAsync(accounts.FirstOrDefault());
                    this.ResultText.Text = "User has signed-out";
                    this.CallGraphButton.Visibility = Visibility.Visible;
                    this.SignOutButton.Visibility = Visibility.Collapsed;
                }
                catch (MsalException ex)
                {
                    ResultText.Text = $"Error signing-out user: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Display basic information contained in the token
        /// </summary>
        private void DisplayBasicTokenInfo(AuthenticationResult authResult)
        {
            TokenInfoText.Text = "";
            if (authResult != null)
            {
                TokenInfoText.Text += $"Username: {authResult.Account.Username}" + Environment.NewLine;
                TokenInfoText.Text += $"Token Expires: {authResult.ExpiresOn.ToLocalTime()}" + Environment.NewLine;
            }
        }
    }
}
