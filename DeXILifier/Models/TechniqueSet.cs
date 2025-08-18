namespace DX9ShaderHLSLifier.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [System.Serializable]
    internal struct TechniqueSet
    {
        public int version;
        public string name;
        public Dictionary<string, string> techniques;

        public bool GetTechnique(MaterialTechniqueType technique, out string techniqueName)
        {
            string index = ((int)technique).ToString();

            if (techniques.TryGetValue(index, out  techniqueName) && !string.IsNullOrEmpty(techniqueName)) {

                return true;
            }

            return false;
        }
    }
}
