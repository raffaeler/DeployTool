using System;
using System.Collections.Generic;
using System.Text;
using DeployTool.Configuration;
using DeployTool.Helpers;

namespace DeployTool.Executers
{
    public class PipelineBag
    {
        public static readonly string PublishDir = "$(publishdir)";
        public static readonly string ProjectDir = "$(projectdir)";
        public static readonly string ProjectName = "$(projectname)";
        public static readonly string AssemblyName = "$(assemblyname)";

        public PipelineBag()
        {
            Bag = new Dictionary<string, object>();
        }

        public bool? IsSuccess { get; set; }

        public string Output { get; set; }

        public IDictionary<string, object> Bag { get; set; }

        public bool GetSshOrFail(out SshTransfer ssh)
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

        /// <summary>
        /// Expand the variables inside the string
        /// If there is no value for a variable, the variable is removed
        /// The values for the variables are taken from the bag
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Expand(string value, bool failIfNotFound)
        {
            if (string.IsNullOrEmpty(value)) return value;
            var sb = new StringBuilder(value.Length);
            var temp = new StringBuilder(value.Length);
            bool isMark1 = false;
            bool isMark2 = false;
            char Mark1 = '$';
            char Mark2 = '(';
            char Mark3 = ')';

            foreach (var ch in value)
            {
                if (isMark2)
                {
                    temp.Append(ch);
                    if (ch == Mark3)
                    {
                        isMark2 = false;
                        // compare with known vars
                        var variable = temp.ToString();
                        if (TryGet(variable, out string variableValue))
                        {
                            sb.Append(variableValue);
                        }
                        else
                        {
                            if (failIfNotFound)
                            {
                                throw new Exception($"The variable {variable} cannot be found");
                            }
                        }

                        temp.Clear();
                    }

                    continue;
                }

                if (isMark1)
                {
                    if (ch == Mark2)
                    {
                        temp.Append(ch);
                        isMark2 = true;
                        continue;
                    }

                    isMark1 = false;
                    sb.Append(temp.ToString());
                    temp.Clear();
                }

                if (ch == Mark1)
                {
                    isMark1 = true;
                    temp.Append(ch);
                    continue;
                }

                sb.Append(ch);
            }

            if (temp.Length > 0)
                sb.Append(temp.ToString());

            return sb.ToString();
        }
    }
}
