using UnityEngine;

public static class ShieldUtility
{
    public static bool IsBlockedByShield(Vector2 attackOrigin, Enemy target, out ShieldDrone drone)
    {
        drone = target.GetComponent<ShieldDrone>();
        if (drone == null) return false;
        if (!drone.enabled) return false;
        // shield active?
        var shieldActiveField = typeof(ShieldDrone).GetField("_shieldActive", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (shieldActiveField != null && shieldActiveField.GetValue(drone) is bool active && !active) return false;

        if (drone.shieldTransform == null) return false;
        Vector2 shieldPos = drone.shieldTransform.position;
        Vector2 shieldForward = (shieldPos - (Vector2)drone.transform.position).normalized;
        Vector2 toOrigin = attackOrigin - (Vector2)drone.transform.position;
        float angle = Vector2.Angle(shieldForward, toOrigin);
        return angle <= drone.shieldHalfAngle;
    }
} 