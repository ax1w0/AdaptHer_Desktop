namespace AdaptHER.Class
{
    public sealed class ComboBoxValueItem<T>
    {
        public string DisplayText { get; set; }
        public T Value { get; set; }
        public override string ToString()
        {
            return DisplayText;
        }
    }
}
