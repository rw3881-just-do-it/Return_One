using UnityEngine;

namespace UnityTV.Player
{
    [System.Serializable]
    public abstract class CareerGoal
    {
        public CareerType CareerType { get; protected set; }
        public string CareerName { get; protected set; }
        public string Description { get; protected set; }

        public int RequiredStrength { get; protected set; }
        public int RequiredIntelligence { get; protected set; }
        public int RequiredAgility { get; protected set; }
        public int RequiredPerception { get; protected set; }
        public int RequiredDexterity { get; protected set; }
        public int RequiredCourage { get; protected set; }

        public virtual bool CheckRequirements(PlayerStats stats)
        {
            return stats.Strength >= RequiredStrength &&
                   stats.Intelligence >= RequiredIntelligence &&
                   stats.Agility >= RequiredAgility &&
                   stats.Perception >= RequiredPerception &&
                   stats.Dexterity >= RequiredDexterity &&
                   stats.Courage >= RequiredCourage;
        }

        public virtual float GetProgressPercentage(PlayerStats stats)
        {
            float Total(int current, int required) =>
                required > 0 ? Mathf.Clamp01((float)current / required) : 1f;

            float sum = Total(stats.Strength, RequiredStrength)
                      + Total(stats.Intelligence, RequiredIntelligence)
                      + Total(stats.Agility, RequiredAgility)
                      + Total(stats.Perception, RequiredPerception)
                      + Total(stats.Dexterity, RequiredDexterity)
                      + Total(stats.Courage, RequiredCourage);

            return sum / 6f;
        }

        public virtual string GetRequirementsText(PlayerStats stats)
        {
            string text = $"Career Goal: {CareerName}\n\nRequirements:\n";

            if (RequiredStrength > 0)
                text += $"Strength: {stats.Strength}/{RequiredStrength}\n";
            if (RequiredIntelligence > 0)
                text += $"Intelligence: {stats.Intelligence}/{RequiredIntelligence}\n";
            if (RequiredAgility > 0)
                text += $"Agility: {stats.Agility}/{RequiredAgility}\n";
            if (RequiredPerception > 0)
                text += $"Perception: {stats.Perception}/{RequiredPerception}\n";
            if (RequiredDexterity > 0)
                text += $"Dexterity: {stats.Dexterity}/{RequiredDexterity}\n";
            if (RequiredCourage > 0)
                text += $"Courage: {stats.Courage}/{RequiredCourage}\n";

            return text;
        }
    }  // <-- CareerGoal abstract class ends HERE

    public class DoctorCareer : CareerGoal
    {
        public DoctorCareer()
        {
            CareerType = CareerType.Doctor;
            CareerName = "Doctor (医生)";
            Description = "俗话说得好,什么时代都少不了医生,就连现在这个时代也一样!";

            RequiredStrength = 0;
            RequiredIntelligence = 70;
            RequiredAgility = 0;
            RequiredPerception = 50;
            RequiredDexterity = 60;
            RequiredCourage = 40;
        }
    }

    public class PoliceCareer : CareerGoal
    {
        public PoliceCareer()
        {
            CareerType = CareerType.Police;
            CareerName = "Police Officer (警察)";
            Description = "一份需要坚定信仰的光辉职业!没有一腔热血和强健的体魄可是当不了警察的哦!";

            RequiredStrength = 60;
            RequiredIntelligence = 40;
            RequiredAgility = 60;
            RequiredPerception = 50;
            RequiredDexterity = 0;
            RequiredCourage = 70;
        }
    }

    public class OfficeWorkerCareer : CareerGoal
    {
        public OfficeWorkerCareer()
        {
            CareerType = CareerType.OfficeWorker;
            CareerName = "Office Worker (公司职员)";
            Description = "平平淡淡才是真,但现在就业形势不景气,想要平平淡淡也得拼尽全力呢";

            RequiredStrength = 0;
            RequiredIntelligence = 50;
            RequiredAgility = 0;
            RequiredPerception = 40;
            RequiredDexterity = 60;
            RequiredCourage = 30;
        }
    }

    public class MerchantCareer : CareerGoal
    {
        public MerchantCareer()
        {
            CareerType = CareerType.Merchant;
            CareerName = "Merchant (行商)";
            Description = "当今这个时代催生出的特别行业,无论是城内还是城外,如果没有行商,人们就没有办法像现在这样生活了";

            RequiredStrength = 30;
            RequiredIntelligence = 50;
            RequiredAgility = 50;
            RequiredPerception = 70;
            RequiredDexterity = 40;
            RequiredCourage = 40;
        }
    }

    public class ScientistCareer : CareerGoal
    {
        public ScientistCareer()
        {
            CareerType = CareerType.Scientist;
            CareerName = "Scientist (科学家)";
            Description = "追求真理的崇高职业 [第一周目无法解锁]";

            RequiredStrength = 0;
            RequiredIntelligence = 80;
            RequiredAgility = 0;
            RequiredPerception = 60;
            RequiredDexterity = 70;
            RequiredCourage = 30;
        }

        public override bool CheckRequirements(PlayerStats stats)
        {
            bool firstPlaythroughComplete = false; // TODO: Get from SaveManager

            if (!firstPlaythroughComplete)
            {
                Debug.Log("Scientist career locked - complete first playthrough!");
                return false;
            }

            return base.CheckRequirements(stats);
        }
    }

    public static class CareerGoalFactory
    {
        public static CareerGoal CreateGoal(CareerType careerType)
        {
            switch (careerType)
            {
                case CareerType.Doctor: return new DoctorCareer();
                case CareerType.Police: return new PoliceCareer();
                case CareerType.OfficeWorker: return new OfficeWorkerCareer();
                case CareerType.Merchant: return new MerchantCareer();
                case CareerType.Scientist: return new ScientistCareer();
                default:
                    Debug.LogWarning($"Unknown career type: {careerType}, defaulting to Office Worker");
                    return new OfficeWorkerCareer();
            }
        }
    }
}