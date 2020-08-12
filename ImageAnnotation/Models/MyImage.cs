using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageAnnotation.Models
{
	public class MyImage
	{
		public string Name { get; set; }
		public short IsDirty { get; set; }
		public short HasLabel { get; set; }
		public short IsDamaged { get; set; }
	}
}
