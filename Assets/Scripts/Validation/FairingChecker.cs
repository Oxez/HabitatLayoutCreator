// Author Oxe
// Created at 04.10.2025 13:28

using UnityEngine;

public static class FairingChecker
{
    public struct Result
    {
        public bool   fits;
        public string message;
    }

    public static Result Check(MissionPreset mission, HabitatShapePreset shape)
    {
        if (mission == null || shape == null)
            return new Result{ fits = true, message = "No presets" };

        float D  = shape.Diameter;
        float L  = shape.Length;
        float FD = mission.MaxFairingDiameterM;
        float FH = mission.MaxFairingHeightM;

        bool   fits;
        string why;

        if (shape.Stow == StowOrientation.Vertical)
        {
            fits = (D <= FD + 1e-3f) && (L <= FH + 1e-3f);
            why  = $"Vertical: D={D:0.0}≤{FD:0.0}, L={L:0.0}≤{FH:0.0}";
        }
        else
        {
            fits = (D <= FH + 1e-3f) && (L <= FD + 1e-3f);
            why  = $"Horizontal: D≤FH & L≤FD → D={D:0.0}≤{FH:0.0}, L={L:0.0}≤{FD:0.0}";
        }

        return new Result {
            fits = fits,
            message = fits ? $"<color=#7CFC00>Fairing OK</color> ({why})"
                : $"<color=#FF5555>Fairing FAIL</color> ({why})"
        };
    }
}
