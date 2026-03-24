using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(JointConverter))]
public class JointConverterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        JointConverter converter = (JointConverter)target;

        if (GUILayout.Button("Convert Joints"))
        {
            converter.ConvertJoints();
        }
    }
}

public class JointConverter : MonoBehaviour
{
    public void ConvertJoints()
    {
        CharacterJoint[] characterJoints = GetComponentsInChildren<CharacterJoint>();

        foreach (CharacterJoint cJoint in characterJoints)
        {
            GameObject go = cJoint.gameObject;

            var connectedBody = cJoint.connectedBody;
            var anchor = cJoint.anchor;
            var axis = cJoint.axis;
            var swingAxis = cJoint.swingAxis;

            DestroyImmediate(cJoint);
            ConfigurableJoint confJoint = go.AddComponent<ConfigurableJoint>();

            // Configuration de base
            confJoint.configuredInWorldSpace = true;
            confJoint.connectedBody = connectedBody;
            confJoint.anchor = anchor;
            confJoint.axis = axis;
            confJoint.secondaryAxis = swingAxis;

            // Verrouiller TOUS les mouvements linéaires
            confJoint.xMotion = ConfigurableJointMotion.Locked;
            confJoint.yMotion = ConfigurableJointMotion.Locked;
            confJoint.zMotion = ConfigurableJointMotion.Locked;

            // Garder les rotations libres
            confJoint.angularXMotion = ConfigurableJointMotion.Free;
            confJoint.angularYMotion = ConfigurableJointMotion.Free;
            confJoint.angularZMotion = ConfigurableJointMotion.Free;

            // Désactivation des drives pour garder l'effet ragdoll
            var emptyDrive = new JointDrive
            {
                positionSpring = 0,
                positionDamper = 0,
                maximumForce = 0
            };

            confJoint.angularXDrive = emptyDrive;
            confJoint.angularYZDrive = emptyDrive;
            confJoint.slerpDrive = emptyDrive;
        }
    }
}
