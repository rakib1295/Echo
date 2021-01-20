using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Echo
{
    class PhoneNumber
    {
        public PhoneNumber()
        {
            DownEntities = new List<Entity>();
            UpEntities = new List<Entity>();
        }
        public int Phone
        { get; set; }
        public List<Entity> DownEntities { get; set; } = new List<Entity>();
        public List<Entity> UpEntities { get; set; } = new List<Entity>();

    }
}
