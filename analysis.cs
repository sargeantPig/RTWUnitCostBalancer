using RTWLib.Functions;
using RTWLib.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RTWLib.Functions.EDU;

namespace RTWTools
{
	public class AnalysisData
	{
		public float attkAverage { get; set; }
		public float defAverage { get; set; }
		public float costAverage { get; set; }
		public int costMin { get; set; }
		public int costMax { get; set; }
		public float upkeepAverage { get; set; }
		public int upkeepMax { get; set; }
		public int upkeepMin { get; set; }
		public int atkMax { get; set; }
		public int atkMin { get; set; }
		public int defMax { get; set; }
		public int defMin { get; set; }
		public float healthAverage { get; set; }
		public int healthMax { get; set; }
		public int healthMin { get; set; }

		public AnalysisData()
		{
			atkMax = 0;
			atkMin = 9999;
			attkAverage = 0;
			healthAverage = 0;
			healthMax = 0;
			healthMin = 9999;
			defAverage = 0;
			defMax = 0;
			defMin = 9999;
			upkeepAverage = 0;
			upkeepMax = 0;
			upkeepMin = 9999;
			costAverage = 0;
			costMax = 0;
			costMin = 9999;
		}

		public static AnalysisData operator -(AnalysisData a, AnalysisData b)
		{
			AnalysisData dd = new AnalysisData();

			/*dd.atk_max = a.atk_max - b.atk_max;
			dd.atk_min = a.atk_min - b.atk_min;
			dd.attk_average = a.attk_average - b.attk_average;
			dd.cost_average = a.cost_average - b.cost_average;
			dd.cost_max = a.cost_max - b.cost_max;
			dd.cost_min = a.cost_min - b.cost_min;
			dd.def_average = a.def_average - b.def_average;
			dd.def_max = a.def_max - b.def_max;
			dd.def_min = a.def_min - b.def_min;
			dd.health_average = a.health_average - b.health_average;
			dd.health_max = a.health_max - b.health_max;
			dd.health_min = a.health_min - b.health_min;
			dd.upkeep_average = a.upkeep_average - b.upkeep_average;
			dd.upkeep_max = a.upkeep_max - b.upkeep_max;
			dd.upkeep_min = a.upkeep_min - b.upkeep_min;*/

			return dd;

		}

		public void Analyse(EDU edu)
		{
			AnalysisData ta = new AnalysisData();
			foreach (Unit unit in edu.units)
			{
				ta.attkAverage += unit.primaryWeapon.attack[0];
				ta.defAverage += unit.primaryArmour.stat_pri_armour[2];
				ta.healthAverage += unit.heatlh[0];
				ta.upkeepAverage += unit.cost[2];
				ta.costAverage += unit.cost[1];

				if (unit.primaryWeapon.attack[0] > atkMax)
					atkMax = unit.primaryWeapon.attack[0];
				else if (unit.primaryWeapon.attack[0] < atkMin)
					atkMin = unit.primaryWeapon.attack[0];
				if (unit.primaryArmour.stat_pri_armour[2] > defMax)
					defMax = unit.primaryArmour.stat_pri_armour[2];
				else if (unit.primaryArmour.stat_pri_armour[2] < defMin)
					defMin = unit.primaryArmour.stat_pri_armour[2];
				if (unit.heatlh[0] > healthMax)
					healthMax = unit.heatlh[0];
				else if (unit.heatlh[0] < healthMin)
					healthMin = unit.heatlh[0];
				if (unit.cost[1] > costMax && unit.cost[1] < 50000)
					costMax = unit.cost[1];
				else if (unit.cost[1] < costMin)
					costMin = unit.cost[1];
				if (unit.cost[2] > upkeepMax)
					upkeepMax = unit.cost[2];
				else if (unit.cost[2] < upkeepMin)
					upkeepMin = unit.cost[2];
			}
			int count = edu.units.Count();
			attkAverage = ta.attkAverage / count;
			costAverage = ta.costAverage / count;
			upkeepAverage = ta.upkeepAverage / count;
			defAverage = ta.defAverage / count;
			healthAverage = ta.healthAverage / count;
		}

		public string Print()
		{
			FieldInfo[] fi = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
			string output = "";
			int ia = 0;
			float i = 0;
			foreach (FieldInfo info in fi)
			{
				string[] nameSplit = info.Name.Split('_');


				if (info.FieldType == ia.GetType())
				{
					ia = (int)info.GetValue(this);
					output += String.Format(nameSplit[0] + ": {0}\r\n", new string[] { ia.ToString() });
				}
				else if (info.FieldType == i.GetType())
				{
					i = (float)info.GetValue(this);
					output += String.Format(nameSplit[0] + ": {0}\r\n", new string[] { i.ToString() });
				}
			}
			return output;


		}
	}
}
