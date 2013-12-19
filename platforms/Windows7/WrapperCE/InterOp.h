#pragma once

namespace WrapperCE
{
	namespace InterOp 
	{
		public enum class UserGender { Male, Female };

		public enum class Arm { LeftArm, RightArm };

		public value struct Point3D
		{
		public:
			double X;
			double Y;
			double Z;
		};

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
			InterOp::Arm TargetArm;

			double Theta;
			InterOp::Vector3D CenterOfMass;

			InterOp::Vector3D Displacement;
			InterOp::Vector3D Velocity;
			InterOp::Vector3D Acceleration;
			double AngularAcceleration;
			InterOp::Vector3D InertialTorque;

			double ShoulderTorque;
			double Endurance;

			double AvgShoulderTorque;
			double AvgEndurance;

			double ConsumedEndurance;
		};

		public value struct ArmFatigueUpdate
		{
		public:
			InterOp::FatigueData LeftArm;
			InterOp::FatigueData RightArm;
		};

		public value struct SkeletonData
		{
		public:
			InterOp::Point3D RightShoulderCms;
			InterOp::Point3D RightElbowCms;
			InterOp::Point3D RightHandCms;
			InterOp::Point3D LeftShoulderCms;
			InterOp::Point3D LeftElbowCms;
			InterOp::Point3D LeftHandCms;
		};
	}
}