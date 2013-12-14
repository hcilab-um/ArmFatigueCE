// This is the main DLL file.

#include "stdafx.h"

#include "WrapperCE.h"

namespace WrapperCE 
{
	EngineCE::EngineCE()
	{
		engineCE = new FatigueEngine();
	}

	EngineCE::~EngineCE()
	{
		delete engineCE;
	}

	WrapperCE::InterOp::ArmFatigueUpdate EngineCE::ProcessNewSkeletonData(WrapperCE::InterOp::SkeletonData armsData)
	{
		//general.h SkeletonData
		SkeletonData input;
		input.rightShoulderCms = Convert(armsData.rightShoulderCms);
		input.rightElbowCms = Convert(armsData.rightElbowCms);
		input.rightHandCms = Convert(armsData.rightHandCms);
		input.leftShoulderCms = Convert(armsData.leftShoulderCms);
		input.leftElbowCms = Convert(armsData.leftElbowCms);
		input.leftHandCms = Convert(armsData.leftHandCms);
		ArmFatigueUpdate update = engineCE->ProcessNewSkeletonData(input, 0);

		WrapperCE::InterOp::ArmFatigueUpdate interOp = WrapperCE::InterOp::ArmFatigueUpdate();
		interOp.LeftArm.Theta = update.LeftArm.Theta;
		return interOp;
	}

	Vector3D EngineCE::Convert(WrapperCE::InterOp::Vector3D source)
	{
		return Vector3D(source.X, source.Y, source.Z);
	}
}


//struct SkeletonData
//{
//	Point3D rightShoulderCms;
//	Point3D rightElbowCms;
//	Point3D rightHandCms;
//	Point3D leftShoulderCms;
//	Point3D leftElbowCms;
//	Point3D leftHandCms;
//};