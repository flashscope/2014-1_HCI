#pragma once
#include "MovingObject.h"

class Album : public MovingObject
{
public:
	Album( std::string imagePath, float positionX, float positionY, float angle, int group );
	virtual ~Album(void);

	virtual void	Draw();
};

