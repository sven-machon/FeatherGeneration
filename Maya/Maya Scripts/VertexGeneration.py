import maya.cmds as cmds

featherDensity = 20

object = cmds.ls(orderedSelection=True)[0]

locatorGroup = cmds.group( empty=True, name = 'Root_Location_grp#')

for i in range (0,featherDensity):
	print('number:  %s' % i)
	
