// This is the main DLL file.

#include "stdafx.h"

#include "WrapperCE.h"

WrapperCE::EngineCE::EngineCE()
{
	engineCE = new FatigueEngine();
	engineCE->SetGender(Male);
}

WrapperCE::EngineCE::~EngineCE()
{
	delete engineCE;
}

void WrapperCE::EngineCE::Reset()
{
	engineCE->Reset();

}
	
void WrapperCE::EngineCE::SetGender(InterOp::UserGender gender)
{
	engineCE->SetGender((UserGender)gender);
}

Vector3D WrapperCE::EngineCE::ConvertPV(InterOp::Point3D source)
{
	return Vector3D(source.X, source.Y, source.Z);
}

WrapperCE::InterOp::Vector3D WrapperCE::EngineCE::ConvertV(Vector3D source)
{
	WrapperCE::InterOp::Vector3D vector3D = WrapperCE::InterOp::Vector3D();
	vector3D.X = source.X;
	vector3D.Y = source.Y;
	vector3D.Z = source.Z;
	return vector3D;
}

WrapperCE::InterOp::Point3D WrapperCE::EngineCE::ConvertVP(Vector3D source)
{
	WrapperCE::InterOp::Point3D point3D = WrapperCE::InterOp::Point3D();
	point3D.X = source.X;
	point3D.Y = source.Y;
	point3D.Z = source.Z;
	return point3D;
}

WrapperCE::InterOp::FatigueData WrapperCE::EngineCE::ConvertFatigueData(FatigueData fatigueData)
{
	WrapperCE::InterOp::FatigueData interOpData = WrapperCE::InterOp::FatigueData();
	interOpData.TargetArm = (InterOp::Arm)fatigueData.TargetArm;
	interOpData.Theta = fatigueData.Theta;
	interOpData.CenterOfMass = this->ConvertV(fatigueData.CenterOfMass);
	interOpData.Displacement = this->ConvertV(fatigueData.Displacement);
	interOpData.Velocity = this->ConvertV(fatigueData.Velocity);
	interOpData.Acceleration = this->ConvertV(fatigueData.Acceleration);
	interOpData.InertialTorque = this->ConvertV(fatigueData.InertialTorque);
	interOpData.AngularAcceleration = fatigueData.AngularAcceleration;
	interOpData.ShoulderTorque = fatigueData.ShoulderTorque;
	interOpData.Endurance = fatigueData.Endurance;
	interOpData.AvgShoulderTorque = fatigueData.AvgShoulderTorque;
	interOpData.ArmStrength = fatigueData.ArmStrength;
	interOpData.AvgArmStrength = fatigueData.AvgArmStrength;
	interOpData.AvgEndurance = fatigueData.AvgEndurance;
	interOpData.ConsumedEndurance = fatigueData.ConsumedEndurance;
	return interOpData;
}

WrapperCE::InterOp::Point3D WrapperCE::EngineCE::EstimateWristPosition(InterOp::Point3D handP, InterOp::Point3D elbowP)
{
	Vector3D hand = this->ConvertPV(handP);
	Vector3D elbow = this->ConvertPV(elbowP);
	return ConvertVP(engineCE->EstimateWristPosition(hand, elbow));
}

WrapperCE::InterOp::ArmFatigueUpdate WrapperCE::EngineCE::ProcessNewSkeletonData(InterOp::SkeletonData armsData, double deltaTimeInSeconds)
{
	InterOp::ArmFatigueUpdate interOp = InterOp::ArmFatigueUpdate();
	//general.h SkeletonData
	SkeletonData input;
	input.RightShoulderCms =	this->ConvertPV(armsData.RightShoulderCms);
	input.RightElbowCms =			this->ConvertPV(armsData.RightElbowCms);
	input.RightWristCms =			this->ConvertPV(armsData.RightWristCms);
	input.RightHandCms =			this->ConvertPV(armsData.RightHandCms);

	input.LeftShoulderCms =		this->ConvertPV(armsData.LeftShoulderCms);
	input.LeftElbowCms =			this->ConvertPV(armsData.LeftElbowCms);
	input.LeftWristCms =			this->ConvertPV(armsData.LeftWristCms);
	input.LeftHandCms =				this->ConvertPV(armsData.LeftHandCms);

	ArmFatigueUpdate update = engineCE->ProcessNewSkeletonData(input, deltaTimeInSeconds);
	interOp.LeftArm = this-> ConvertFatigueData(update.LeftArm);
	interOp.RightArm = this-> ConvertFatigueData(update.RightArm);

	return interOp;
}