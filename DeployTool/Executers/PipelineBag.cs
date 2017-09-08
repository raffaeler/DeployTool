using System;
using System.Collections.Generic;
using System.Text;
using DeployTool.Configuration;

namespace DeployTool.Executers
{
    public class PipelineBag
    {
        public PipelineBag()
        {
            Bag = new Dictionary<string, object>();
        }

        public bool IsSuccess { get; set; }

        public string Output { get; set; }

        public IDictionary<string, object> Bag { get; set; }

        public bool GetSshOrFail(out SshConfiguration ssh)
        {
            if (!TryGet("ssh", out ssh))
            {
                SetError("Missing ssh configuration");
                return false;
            }

            return true;
        }

        public bool TryGet<T>(string property, out T value)
        {
            if (Bag.TryGetValue(property, out object val))
            {
                value = (T)val;
                return true;
            }

            value = default(T);
            return false;
        }

        public void SetValue<T>(string propertyName, T value)
        {
            Bag[propertyName] = value;
        }

        public void SetResult(bool isError, string output)
        {
            IsSuccess = !isError;
            this.Output = output;
        }

        public void SetSuccess(string output)
        {
            IsSuccess = true;
            this.Output = output;
        }

        public void SetError(string output)
        {
            IsSuccess = false;
            this.Output = output;
        }
    }
}
