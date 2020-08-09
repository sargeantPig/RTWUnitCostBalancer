using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using RTWLib.Data;
using RTWLib.Functions.EDU;
using RTWLib.Objects;
namespace RTWUnitCostBalancer
{
    public class Balancer
    {
        int baseAttack;
        int baseCharge;
        int baseArmourValue;
        int baseDefenceSkill;
        int baseShieldSkill;
        int baseMoraleValue;



        public Balancer(int baseAttack = 5, int baseCharge = 4, int baseArmourValue = 4, 
            int baseDefenceSkill = 5, int baseShieldSkill = 2, int baseMoraleSkill = 7)
        {
            this.baseAttack = baseAttack;
            this.baseCharge = baseCharge;
            this.baseArmourValue = baseArmourValue;
            this.baseDefenceSkill = baseDefenceSkill;
            this.baseMoraleValue = baseMoraleSkill;
            this.baseShieldSkill = baseShieldSkill;

        }

        public float CalculateCost(Unit unit)
        {
            return (float)Math.Round(
                        GetSiegeVal(unit, 0.4f, 1.0f) *                                     //multiplier for siege engines
                        (unit.soldier.number / 40) *                                             //quantity multiplier
                        ((GetPriAttk(unit) - baseAttack) * 50                                             //primary attack, 5 is base attack, can be a variable
                        + (unit.primaryWeapon.attack[1] - baseCharge) * 20                                //primary charge; 4 is a base charge, can be a variable
                        + unit.primaryWeapon.Missleattri[0] * 2                                //missile distance factor
                        + GetAmmoValue(unit) * (GetPriAttk(unit) - 5)                            //number of missiles factor
                        + GetAP_Pri(unit) * 20                                                //factor from armor piercing attribute
                        + GetSecAttkCostInfluence(unit)                                        //separate calculations for secondary weapon if exists
                        + (unit.primaryArmour.stat_pri_armour[0] - baseArmourValue) * 40                                            //should be (unit.primaryArmour.stat_pri_armour[0]-4)*40    4 is a base armor value, can be a variable
                        + (unit.primaryArmour.stat_pri_armour[1] - baseDefenceSkill) * 20                        //5 is a base defence skill value, can be a variable
                        + (unit.primaryArmour.stat_pri_armour[2] - baseShieldSkill) * 20                        //2 is a base shield value, can be a variable
                        + (unit.mental.morale - baseMoraleValue) * 10                                            //7 is a base morale value, can be a variable
                        + GetTrainingValue(unit) * 20)                                        //training factor. 0 for untrained, 1 for trained, 2 for highly trained        
                        , 0);
            /* return (float)Math.Round(GetSiegeVal(unit, 0.4f, 1.0f) * ((unit.soldier.number - 40) * 10+(GetPriAttk(unit)-5)*50 +(unit.primaryWeapon.attack[1]-4) 
                 * 20+unit.primaryWeapon.Missleattri[0]*2+GetAmmoValue(unit)+GetAP_Pri(unit)*20+GetSecAttkCostInfluence(unit)
                 +(GetAmmoValue(unit)-4)*40+(unit.primaryArmour.stat_pri_armour[1]-5)*20+(unit.primaryArmour.stat_pri_armour[2]-2)*20+(unit.mental.morale-7)*10+GetTrainingValue(unit)*20), 0);*/
        }

        public float CalculateUpkeep(Unit unit)
        {
            return (int)Math.Round(CalculateCost(unit)/10.0f) * GetMercenaryVal(unit);
        }

        public float CalculateWepUpgrade(Unit unit)
        {
            return (float)Math.Round(GetSiegeVal(unit, 1.0f, 1.1f) * (GetWepTypeValuePri(unit) * (GetPriAttk(unit) - 5) * 10 + GetAP_Pri(unit) * 2) * GetAmmoValue(unit) + GetSecondaryAttkMod(unit), 0);
        }

        public float CalculateCustomCost(Unit unit)
        { 
            return (int)Math.Round(unit.cost[1] / 4.0);
        }

        public float CalculateArmourUpgradeCost(Unit unit)
        {
            return (unit.primaryArmour.stat_pri_armour[1] - 4) * (50 + 50);
        }

        float GetTrainingValue(Unit unit)
        {
            switch (unit.mental.training)
            {
                case Statmental_training.highly_trained: return 2f;
                case Statmental_training.trained: return 1f;
                case Statmental_training.untrained: return 0f;
                default: return 0;
            }
        }

        float GetWepTypeValuePri(Unit unit)
        {
            switch (unit.primaryWeapon.TechFlags)
            {
                case RTWLib.Data.TechType.archery:
                    return 2.0f;
                case RTWLib.Data.TechType.blade:
                    return 3.0f;
                case RTWLib.Data.TechType.simple:
                    return 1.0f;
                case RTWLib.Data.TechType.siege:
                    return 2.0f;
                case RTWLib.Data.TechType.other:
                    return 2.0f;
                default: return 0.0f;
            }
        }


        float GetWepTypeValueSec(Unit unit)
        {
            switch (unit.secondaryWeapon.TechFlags)
            {
                case RTWLib.Data.TechType.archery:
                    return 2.0f;
                case RTWLib.Data.TechType.blade:
                    return 3.0f;
                case RTWLib.Data.TechType.simple:
                    return 1.0f;
                case RTWLib.Data.TechType.siege:
                    return 2.0f;
                case RTWLib.Data.TechType.other:
                    return 2.0f;
                default: return 0.0f;
            }
        }

        float GetAmmoValue(Unit unit)
        {
            if (unit.primaryWeapon.Missleattri[1] > 10)
                return unit.primaryWeapon.Missleattri[1] / 10;
            else return unit.primaryWeapon.Missleattri[1];
        }

        bool isSiege(Unit unit)
        {
            if (unit.engine != null)
                return true;
            else return false;
        }

        float GetSiegeVal(Unit unit, float tru, float fal )
        {
            if (isSiege(unit))
                return tru;
            else return fal;
        }

        float GetAP_Pri(Unit unit)
        {
            if (unit.priAttri.HasFlag(Stat_pri_attr.ap))
                return 1.0f;
            else return 0.0f;
        }

        float GetAP_Sec(Unit unit)
        {
            if (unit.secAttri.HasFlag(Stat_pri_attr.ap))
                return 1.0f;
            else return 0.0f;
        }

        float GetPriAttk(Unit unit)
        {
            if (isSiege(unit))
                return unit.primaryWeapon.attack[0];
            else return unit.primaryWeapon.attack[0];
        }

        float GetSecondaryAttkMod(Unit unit)
        {
            if (unit.secondaryWeapon.attack[0] > 0)
                return GetWepTypeValueSec(unit) * (GetWepTypeValueSec(unit) - 5) + GetAP_Sec(unit) * 2;
            else
                return 0;
        }

        float GetSecAttkCostInfluence(Unit unit)
        {
            if (unit.secondaryWeapon.attack[0] > 0)
                return (unit.secondaryWeapon.attack[0] - 5) * 10 + (unit.secondaryWeapon.attack[1] - 4) * 10 + GetAP_Sec(unit) * 10;
            else return 0;
        }

        float GetMercenaryVal(Unit unit)
        {
            if (isMercenary(unit))
                return 1;
            else return 2;
        }

        bool isMercenary(Unit unit)
        {
            if (unit.attributes.HasFlag(Attributes.mercenary_unit))
                return true;
            else return false;
        }



    }
}
