#include "Photo.h"


Photo::Photo( std::string imagePath, float positionX, float positionY, float angle, int group )
	: MovingObject( imagePath, positionX, positionY, angle, group)
{
	m_IsVisible = false;
	m_MOType = MO_TYPE_PHOTO;
}


Photo::~Photo(void)
{
}


void Photo::Draw()
{
	MovingObject::Draw();
}