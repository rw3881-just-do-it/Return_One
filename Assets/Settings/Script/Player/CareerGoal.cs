using UnityEngine;
using UnityEngine.U2D;

namespace UnityTV.Player
{
    /// <summary>
    /// Base class for career goals - defines what stats are needed to achieve each career
    /// </summary>
    [System.Serializable]
    public abstract class CareerGoal
    {
        public CareerType CareerType { get; protected set; }
        public string CareerName { get; protected set; }
        public string Description { get; protected set; }

        // Required stat thresholds to achieve this career
        public int RequiredIntelligence { get; protected set; }
        public int RequiredPhysicalStrength { get; protected set; }
        public int RequiredMentalStrength { get; protected set; }
        public int RequiredIdeal { get; protected set; }

        /// <summary>
        /// Check if player has met all requirements for this career
        /// </summary>
        public virtual bool CheckRequirements(PlayerStats stats)
        {
            return stats.Intelligence >= RequiredIntelligence &&
                   stats.PhysicalStrength >= RequiredPhysicalStrength &&
                   stats.MentalStrength >= RequiredMentalStrength &&
                   stats.Ideal >= RequiredIdeal;
        }

        /// <summary>
        /// Get progress percentage (0-1) towards meeting requirements
        /// </summary>
        public virtual float GetProgressPercentage(PlayerStats stats)
        {
            float intelligenceProgress = RequiredIntelligence > 0
                ? Mathf.Clamp01((float)stats.Intelligence / RequiredIntelligence)
                : 1f;

            float physicalProgress = RequiredPhysicalStrength > 0
                ? Mathf.Clamp01((float)stats.PhysicalStrength / RequiredPhysicalStrength)
                : 1f;

            float mentalProgress = RequiredMentalStrength > 0
                ? Mathf.Clamp01((float)stats.MentalStrength / RequiredMentalStrength)
                : 1f;

            float idealProgress = RequiredIdeal > 0
                ? Mathf.Clamp01((float)stats.Ideal / RequiredIdeal)
                : 1f;

            // Average progress across all requirements
            return (intelligenceProgress + physicalProgress + mentalProgress + idealProgress) / 4f;
        }

        /// <summary>
        /// Get a description of what's still needed
        /// </summary>
        public virtual string GetRequirementsText(PlayerStats stats)
        {
            string text = $"Career Goal: {CareerName}\n\n";
            text += "Requirements:\n";

            if (RequiredIntelligence > 0)
                text += $"Intelligence: {stats.Intelligence}/{RequiredIntelligence}\n";

            if (RequiredPhysicalStrength > 0)
                text += $"Physical Strength: {stats.PhysicalStrength}/{RequiredPhysicalStrength}\n";

            if (RequiredMentalStrength > 0)
                text += $"Mental Strength: {stats.MentalStrength}/{RequiredMentalStrength}\n";

            if (RequiredIdeal > 0)
                text += $"Ideal/Hope: {stats.Ideal}/{RequiredIdeal}\n";

            return text;
        }
    }

    /// <summary>
    /// Doctor career goal - 医生
    /// High intelligence, moderate physical/mental strength
    /// </summary>
    public class DoctorCareer : CareerGoal
    {
        public DoctorCareer()
        {
            CareerType = CareerType.Doctor;
            CareerName = "Doctor (医生)";
            Description = "俗话说得好,什么时代都少不了医生,就连现在这个时代也一样!";

            RequiredIntelligence = 80;
            RequiredPhysicalStrength = 40;
            RequiredMentalStrength = 60;
            RequiredIdeal = 50;
        }
    }

    /// <summary>
    /// Police career goal - 警察
    /// High physical strength, good mental strength and ideal
    /// </summary>
    public class PoliceCareer : CareerGoal
    {
        public PoliceCareer()
        {
            CareerType = CareerType.Police;
            CareerName = "Police Officer (警察)";
            Description = "一份需要坚定信仰的光辉职业!没有一腔热血和强健的体魄可是当不了警察的哦!";

            RequiredIntelligence = 40;
            RequiredPhysicalStrength = 80;
            RequiredMentalStrength = 70;
            RequiredIdeal = 70;
        }
    }

    /// <summary>
    /// Office Worker career goal - 公司职员
    /// Balanced stats, moderate requirements
    /// </summary>
    public class OfficeWorkerCareer : CareerGoal
    {
        public OfficeWorkerCareer()
        {
            CareerType = CareerType.OfficeWorker;
            CareerName = "Office Worker (公司职员)";
            Description = "平平淡淡才是真,但现在就业形势不景气,想要平平淡淡也得拼尽全力呢";

            RequiredIntelligence = 60;
            RequiredPhysicalStrength = 40;
            RequiredMentalStrength = 60;
            RequiredIdeal = 50;
        }
    }

    /// <summary>
    /// Merchant career goal - 行商
    /// High mental strength, good physical strength, moderate intelligence
    /// </summary>
    public class MerchantCareer : CareerGoal
    {
        public MerchantCareer()
        {
            CareerType = CareerType.Merchant;
            CareerName = "Merchant (行商)";
            Description = "当今这个时代催生出的特别行业,无论是城内还是城外,如果没有行商,人们就没有办法像现在这样生活了";

            RequiredIntelligence = 50;
            RequiredPhysicalStrength = 70;
            RequiredMentalStrength = 80;
            RequiredIdeal = 60;
        }
    }

    /// <summary>
    /// Scientist career goal - 科学家
    /// Very high intelligence, locked for first playthrough
    /// </summary>
    public class ScientistCareer : CareerGoal
    {
        public ScientistCareer()
        {
            CareerType = CareerType.Scientist;
            CareerName = "Scientist (科学家)";
            Description = "追求真理的崇高职业 [第一周目无法解锁]";

            RequiredIntelligence = 100;
            RequiredPhysicalStrength = 40;
            RequiredMentalStrength = 80;
            RequiredIdeal = 90;
        }

        public override bool CheckRequirements(PlayerStats stats)
        {
            // TODO: Check if player has completed first playthrough
            bool firstPlaythroughComplete = false; // Get from SaveManager

            if (!firstPlaythroughComplete)
            {
                Debug.Log("Scientist career locked - complete first playthrough!");
                return false;
            }

            return base.CheckRequirements(stats);
        }
    }

    /// <summary>
    /// Factory class to create career goals based on career type
    /// </summary>
    public static class CareerGoalFactory
    {
        public static CareerGoal CreateGoal(CareerType careerType)
        {
            switch (careerType)
            {
                case CareerType.Doctor:
                    return new DoctorCareer();

                case CareerType.Police:
                    return new PoliceCareer();

                case CareerType.OfficeWorker:
                    return new OfficeWorkerCareer();

                case CareerType.Merchant:
                    return new MerchantCareer();

                case CareerType.Scientist:
                    return new ScientistCareer();

                default:
                    Debug.LogWarning($"Unknown career type: {careerType}, defaulting to Office Worker");
                    return new OfficeWorkerCareer();
            }
        }
    }
}