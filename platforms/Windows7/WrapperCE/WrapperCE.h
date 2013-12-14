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
		Vector3D Convert(WrapperCE::InterOp::Vector3D source);

	public:
		EngineCE();
		~EngineCE();

		WrapperCE::InterOp::ArmFatigueUpdate ProcessNewSkeletonData(WrapperCE::InterOp::SkeletonData armsData);
	};
}