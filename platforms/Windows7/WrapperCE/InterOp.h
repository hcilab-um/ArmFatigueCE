namespace WrapperCE
{
	namespace InterOp {

		public enum UserGender { Male, Female };

		public enum Arm { LeftArm, RightArm };

		public value struct Point3D
		{
		public:
			double X;
			double Y;
			double Z;
		};

		#define Vector3D Point3D

		public value struct FatigueData
		{
		public:
			Arm TargetArm;

			double Theta;
			Point3D CenterOfMass;
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
			Point3D rightShoulderCms;
			Point3D rightElbowCms;
			Point3D rightHandCms;
			Point3D leftShoulderCms;
			Point3D leftElbowCms;
			Point3D leftHandCms;
		};
	}
}