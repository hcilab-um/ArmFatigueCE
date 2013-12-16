// This is the main DLL file.

#include "stdafx.h"

#include "WrapperCE.h"

WrapperCE::EngineCE::EngineCE()
{
	engineCE = new FatigueEngine();
}

WrapperCE::EngineCE::~EngineCE()
{
	delete engineCE;
}

Vector3D WrapperCE::EngineCE::ConvertPV(InterOp::Point3D source)
{
	return Vector3D(source.X, source.Y, source.Z);
}

WrapperCE::InterOp::ArmFatigueUpdate WrapperCE::EngineCE::ProcessNewSkeletonData(InterOp::SkeletonData armsData)
{
	//general.h SkeletonData
	SkeletonData input;
	input.RightShoulderCms =	this->ConvertPV(armsData.RightShoulderCms);
	input.RightElbowCms =			this->ConvertPV(armsData.RightElbowCms);
	input.RightHandCms =			this->ConvertPV(armsData.RightHandCms);
	input.LeftShoulderCms =		this->ConvertPV(armsData.LeftShoulderCms);
	input.LeftElbowCms =			this->ConvertPV(armsData.LeftElbowCms);
	input.LeftHandCms =				this->ConvertPV(armsData.LeftHandCms);
	ArmFatigueUpdate update = engineCE->ProcessNewSkeletonData(input, 0);

	InterOp::ArmFatigueUpdate interOp = InterOp::ArmFatigueUpdate();
	interOp.LeftArm.Theta = update.LeftArm.Theta;
	return interOp;
}
