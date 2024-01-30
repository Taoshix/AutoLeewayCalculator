using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticLeewayCalculator
{
    public class Setting
    {
        public int Id { get; set; }
        public string Prefix { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool State { get; set; }

        public Setting()
        {
        }

        public Setting(int id, string prefix, string name, string description, bool state)
        {
            Id = id;
            Prefix = prefix;
            Name = name;
            Description = description;
            State = state;
        }
        public bool ToggleState()
        {
            State = !State;
            return !State;
        }
    }
}
