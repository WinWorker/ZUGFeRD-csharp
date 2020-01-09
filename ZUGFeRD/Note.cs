namespace s2industries.ZUGFeRD
{
    public class Note
    {
        public string Content { get; set; }
        public SubjectCodes SubjectCode { get; set; }
        public ContentCodes ContentCode { get; set; }

        public Note(string content, SubjectCodes subjectCode, ContentCodes contentCode)
        {
            this.Content = content;
            this.SubjectCode = subjectCode;
            this.ContentCode = contentCode;
        }
    }
}
