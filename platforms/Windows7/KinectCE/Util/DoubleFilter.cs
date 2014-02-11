using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectCE.Util
{
	public class DoubleFilter
	{
		 public double StableValue { get; set; }

    private CircularList<double> ValuesBuffer { get; set; }

    public DoubleFilter(int bufferSize)
    {
			ValuesBuffer = new CircularList<double>(bufferSize);
    }

		public double FilterData(double newData)
    {
			ValuesBuffer.Value = newData;
			ValuesBuffer.Next();
			return ValuesBuffer.Sum()/ValuesBuffer.Length;
    }

    public void Reset()
    {
			ValuesBuffer.Clear();
    }
	}
}
