using MoviesDBManager.Models;
using System.Reflection;
using System.Web.Hosting;

namespace ChatManager.Models
{
    public class DB
    {
        private static readonly DB instance = new DB();

        public static UsersRepository Users { get; set; }
        public static RelationshipRepository Relationships { get; set; }
        public static Repository<Gender> Genders { get; set; }
        public static Repository<UserType> UserTypes { get; set; }
        public static Repository<UnverifiedEmail> UnverifiedEmails { get; set; }
        public static Repository<ResetPasswordCommand> ResetPasswordCommands { get; set; }
        public static Repository<Connection> Connections { get; set; }
        public static Repository<Message> Messages { get; set; }

        public DB()
        {
            Users = new UsersRepository();
            Relationships = new RelationshipRepository();
            Genders = new Repository<Gender>();
            UserTypes = new Repository<UserType>();
            UnverifiedEmails = new Repository<UnverifiedEmail>();
            ResetPasswordCommands = new Repository<ResetPasswordCommand>();
            Connections = new Repository<Connection>();
            Messages = new Repository<Message>();

            InitRepositories(this);
        }
        public static DB Instance
        {
            get { return instance; }
        }
        private static void InitRepositories(DB db)
        {
            string serverPath = HostingEnvironment.MapPath(@"~/App_Data/");
            PropertyInfo[] myPropertyInfo = db.GetType().GetProperties();
            foreach (PropertyInfo propertyInfo in myPropertyInfo)
            {
                if (propertyInfo.PropertyType.Name.Contains("Repository"))
                {
                    MethodInfo method = propertyInfo.PropertyType.GetMethod("Init");
                    method.Invoke(propertyInfo.GetValue(db), new object[] { serverPath + propertyInfo.Name + ".json" });
                }
            }
        }
    }
}