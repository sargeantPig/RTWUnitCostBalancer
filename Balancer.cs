using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
        int baseCost;
        int baseHealth;

        public Balancer(int baseAttack = 5, int baseCharge = 4, int baseArmourValue = 4, 
            int baseDefenceSkill = 5, int baseShieldSkill = 2, int baseMoraleSkill = 7, int baseCost = 1000, int baseHealth = 1)
        {
            this.baseAttack = baseAttack;
            this.baseCharge = baseCharge;
            this.baseArmourValue = baseArmourValue;
            this.baseDefenceSkill = baseDefenceSkill;
            this.baseMoraleValue = baseMoraleSkill;
            this.baseShieldSkill = baseShieldSkill;
            this.baseCost = baseCost;
            this.baseHealth = baseHealth;
        }

        public float CalculateCost(Unit unit)
        {
            float cost = (float)Math.Round((
                        GetSiegeVal(unit, 0.4f, 1.0f) *                                      //category. 1 for infantry, 0.4 for siege, 1.2 for cavalry. Ships use a separate formula
                        (unit.soldier.number / 40) *                                           //quantity
                        (GetPriAttk(unit) * baseAttack) *                           //pri attack
                        (unit.primaryWeapon.attack[1] / baseCharge) *               //pri charge
                        Math.Pow(unit.primaryWeapon.Missleattri[0], 1 / 4) *               //missile distance
                        Math.Pow(GetAmmoValue(unit), 1 / 4) *                              //ammo
                        Math.Pow(GetAP_Pri(unit), 1 / 2) *                                 //armor piercing. GetAP_Pri(unit) should return 2 if AP, and 1 if not
                        Math.Pow(GetSecAttkCostInfluence(unit) / baseAttack, 1 / 2) *                             //sec attack ; if no sec, should be 1
                        Math.Pow(unit.secondaryWeapon.attack[1] / baseCharge, 1 / 2) *               //sec charge; if no sec, should be 1
                        unit.primaryArmour.stat_pri_armour[0] / baseArmourValue *                 //pri armor
                        unit.primaryArmour.stat_pri_armour[1] / baseDefenceSkill *             //pri def skill
                        unit.primaryArmour.stat_pri_armour[2] / baseShieldSkill *          //pri shield
                        unit.mental.morale / baseMoraleValue *                             //morale value
                        Math.Pow(GetTrainingValue(unit) * 20, 1 / 4) *                      //training. Function inside returns 0 for untrained, 1 for trained, 2 for highly_trained
                        Math.Pow(unit.secondaryArmour.stat_sec_armour[0] / baseArmourValue, 1 / 4) *               //sec armor
                        Math.Pow(unit.secondaryArmour.stat_sec_armour[1] / baseDefenceSkill, 1 / 4) *           //sec defence skill          //sec shield  
                        Math.Pow(unit.heatlh[0] / baseHealth, 1 / 4) *
                        Math.Pow(unit.heatlh[1] / baseHealth, 1 / 4) *
                        Math.Pow(GetSpearBonus(unit), 1 / 4)) / 10 + 1 *
                        baseCost
                        //base cost. Maybe 500?
                        , 0) ;

            float phalmod = PhalanxModifier(unit, (int)cost);
            cost += phalmod;
            float pmod = precModifier(unit, (int)cost);

            cost += pmod;

            return cost * GetMoraleModifier(unit);
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

        float precModifier(Unit unit, int value)
        {
            if (unit.priAttri.HasFlag(Stat_pri_attr.prec) || unit.secAttri.HasFlag(Stat_pri_attr.prec))
                return 0.25f * value;
            else return 0f;
        }

        float PhalanxModifier(Unit unit, int value)
        {
            if (unit.formation.FormationFlags.HasFlag(FormationTypes.phalanx))
            {
                return 0.20f * value;
            }
            else return 0f;
        }

        int GetSpearBonus(Unit unit)
        {
            switch (unit.priAttri)
            {
                case Stat_pri_attr.spear_bonus_2:
                    return 2;
                case Stat_pri_attr.spear_bonus_4:
                    return 4;
                case Stat_pri_attr.spear_bonus_6:
                    return 6;
                case Stat_pri_attr.spear_bonus_8:
                    return 8;
                case Stat_pri_attr.spear_bonus_10:
                    return 10;
                case Stat_pri_attr.spear_bonus_12:
                    return 12;
                default: break;
            }

            switch (unit.secAttri)
            {
                case Stat_pri_attr.spear_bonus_2:
                    return 2;
                case Stat_pri_attr.spear_bonus_4:
                    return 4;
                case Stat_pri_attr.spear_bonus_6:
                    return 6;
                case Stat_pri_attr.spear_bonus_8:
                    return 8;
                case Stat_pri_attr.spear_bonus_10:
                    return 10;
                case Stat_pri_attr.spear_bonus_12:
                    return 12;
                default: break;
            }

            return 0;
        }

        float GetMoraleModifier(Unit unit)
        {
            switch (unit.mental.discipline)
            {
                case Statmental_discipline.low:
                    return 0.5f;
                case Statmental_discipline.normal:
                    return 0.80f;
                case Statmental_discipline.impetuous:
                    return 1f;
                case Statmental_discipline.disciplined:
                    return 1.2f;
                case Statmental_discipline.berserker:
                    return 1.75f;
                default: return 1f;
            }
        }
    }
}
