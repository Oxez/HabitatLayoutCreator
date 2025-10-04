// Author Oxe
// Created at 04.10.2025 10:55

using UnityEngine;

public class HabitatContext : MonoBehaviour
{
    public MissionPreset Mission;
    public RuleSet       Rules;

    public int CrewSize => Mission != null ? Mission.CrewSize : 4;
    public int MissionDays => Mission != null ? Mission.MissionDays : 180;
}
