namespace SPCoEdit.Dto
{
    public class APIResponse<T>
    {
        public T Data { get; set; }
        public string Error { get; set; }
    }
}
