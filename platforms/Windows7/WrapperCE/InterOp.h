namespace WrapperCE
{
	namespace InterOp {

		public enum UserGender { Male, Female };

		public enum Arm { LeftArm, RightArm };

		public value struct Vector3D
		{
		public:
			double X;
			double Y;
			double Z;
		};

		public value struct FatigueData
		{
		public:
			Arm TargetArm;

			double Theta;
			Vector3D CenterOfMass;
			Vector3D Displacement;
			Vector3D Velocity;
			Vector3D Acceleration;
			Vector3D AngularAcceleration;
			Vector3D InertialTorque;

			double ShoulderTorque;
			double Endurance;

			double AvgShoulderTorque;
			double AvgEndurance;

			double ConsumedEndurance;
		};

		public value struct ArmFatigueUpdate
		{
		public:
			FatigueData LeftArm;
			FatigueData RightArm;
		};

		public value struct SkeletonData
		{
			Vector3D rightShoulderCms;
			Vector3D rightElbowCms;
			Vector3D rightHandCms;
			Vector3D leftShoulderCms;
			Vector3D leftElbowCms;
			Vector3D leftHandCms;
		};
	}
}