namespace AspNetFormatter.Rules
{
    public abstract class FormattingRule
    {
        public abstract bool Validate(string path, string fileContent);
        public abstract string Fix(string path, string fileContent);
    }
}
