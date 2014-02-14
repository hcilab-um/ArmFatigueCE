// WrapperCE.h

#pragma once

#include "ArmFatigueCE.h"
#include "InterOp.h"

using namespace System;

namespace WrapperCE 
{
	public ref class EngineCE
	{
	private:
		FatigueEngine* engineCE;

		Vector3D ConvertPV(InterOp::Point3D source);
		InterOp::Vector3D ConvertV(Vector3D source);
		InterOp::Point3D ConvertVP(Vector3D source);

		InterOp::FatigueData ConvertFatigueData(FatigueData fatigueData);
	public:
		EngineCE();
		~EngineCE();
		void Reset();
		void SetGender(InterOp::UserGender gender);
		WrapperCE::InterOp::Point3D EstimateWristPosition(InterOp::Point3D hand, InterOp::Point3D elbow);
		WrapperCE::InterOp::ArmFatigueUpdate ProcessNewSkeletonData(InterOp::SkeletonData armsData, double deltaTimeInSeconds);
	};
}