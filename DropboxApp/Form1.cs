using Dropbox.Api;
using System.Net;
using System.Text;
using System.Web;
using static Dropbox.Api.TeamLog.ActorLogInfo;


namespace DropboxApp {
    public partial class Form1 : Form {

        String accessToken =
"sl.BrtztcY3kAtb5YpAaZ9DE1GpwnAhrCLurcqOL57RxcH6gljgO5tk6FYVNBwhISBvnwdMw4AqgF9LDtvXVAHg7JN0stfYJrj66xVT4uqYg4YGv-LISyD1F4s_ZYiMV-mqgLs3dER3gVIRqSwBGIdc";
        String AppKey
        = "2srvbrhaas5bv19";
        String AppSecret
        = "zpqetilxcojr7qf";
        private const string RedirectUri = "https://localhost/authorize";
        String strAuthenticationURL;
        String strAccessToken;
        String appName = "PhotoMetaViewer";

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {

        }

        private async void button1_Click(object sender, EventArgs e) {

            String result = await getResult();
            textBox1.Text=result;
        }
        async Task<string> getResult() {
            try
            {
                StringBuilder sb=new StringBuilder();
                var dbx = new DropboxClient(accessToken);

                var full = await dbx.Users.GetCurrentAccountAsync();
                //Console.WriteLine("{0} - {1}", full.Name.DisplayName, full.Email);
                //textBox1.Text = full.Name.DisplayName;
                var list = await dbx.Files.ListFolderAsync(string.Empty);
                foreach (var item in list.Entries.Where(i => i.IsFolder))
                {
                    sb.Append(item.Name);
                    sb.Append("\r\n");

                }
                //return full.Name.DisplayName;

                var result = dbx.Files.SearchV2Async();
                return sb.ToString();
             }
            catch (Exception ex)
            {
                //textBox1.Text = ex.Message;
                return ex.Message;
            }

        }
    
      
    }
}
