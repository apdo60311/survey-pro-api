namespace survey_pro.Settings
{
    public class MongoDbSettings
    {
        public required string ConnectionString { get; set; }
        public required string DatabaseName { get; set; }
        public required string UsersCollection { get; set; }
        public string RolesCollection { get; set; } = "Roles";
        public string SurveysCollection { get; set; } = "Surveys";
        public string ResponsesCollection { get; set; } = "SurveyResponses";
    }
}