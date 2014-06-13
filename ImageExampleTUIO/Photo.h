#pragma once
#include "MovingObject.h"

class Photo : public MovingObject
{
public:
	Photo( std::string imagePath, float positionX, float positionY, float angle, int group );
	virtual ~Photo(void);

	virtual void	Draw();
};

