namespace GH.Core.ViewModels
{
    public class AnswerSecurityQuestionModel
    {
       public AnswerSecurityQuestionModel() {
            Question1 = new AnswerSecurityQuestionViewModel();
            Question2 = new AnswerSecurityQuestionViewModel();
            Question3 = new AnswerSecurityQuestionViewModel();
        }
        public AnswerSecurityQuestionViewModel Question1 { get; set; }
        public AnswerSecurityQuestionViewModel Question2 { get; set; }
        public AnswerSecurityQuestionViewModel Question3 { get; set; }

        public string VerifiedToken { get; set; }
    }

    public class AnswerSecurityQuestionViewModel
    {
        public string QuestionId { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
    }
}