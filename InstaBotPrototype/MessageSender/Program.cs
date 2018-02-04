using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSender
{
    class Program
    {
        static void Main(string[] args)
        {
            var postman = new MessageSender();
            postman.SendMessages();
        }
    }
}
