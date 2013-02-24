using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GpxcApplication
{
    class GpxcOptions
    {
        private bool nuvi = false;
        private bool nuvi2 = false;
        private bool silent = false;

        public bool Nuvi
        {
            set { this.nuvi = value; }
            get { return this.nuvi; }
        }
        public bool Nuvi2
        {
            set { this.nuvi2 = value; }
            get { return this.nuvi2; }
        }
        public bool Silent
        {
            set { this.silent = value; }
            get { return this.silent; }
        }
    }
}
