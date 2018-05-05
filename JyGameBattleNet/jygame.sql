/*
MySQL Data Transfer
Source Host: www.jy-x.com
Source Database: jygame
Target Host: www.jy-x.com
Target Database: jygame
Date: 2013/12/14 15:56:58
*/

SET FOREIGN_KEY_CHECKS=0;
-- ----------------------------
-- Table structure for jy_save
-- ----------------------------
CREATE TABLE `jy_save` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) NOT NULL,
  `index` int(11) NOT NULL,
  `content` text,
  `createtime` datetime DEFAULT NULL,
  `updatetime` timestamp NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `user` (`userid`),
  KEY `usersearch` (`userid`,`index`)
) ENGINE=MyISAM AUTO_INCREMENT=7 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for jy_user
-- ----------------------------
CREATE TABLE `jy_user` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(50) NOT NULL,
  `password` varchar(50) NOT NULL,
  `score` int(11) NOT NULL DEFAULT '1000',
  `createtime` datetime DEFAULT NULL,
  `updatetime` timestamp NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `name` (`name`),
  KEY `id` (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records 
-- ----------------------------
INSERT INTO `jy_save` VALUES ('1', '1', '0', '123123', '2013-12-11 21:18:38', '2013-12-14 15:09:16');
INSERT INTO `jy_save` VALUES ('2', '1', '1', 'Empty55411', '2013-12-11 21:18:41', '2013-12-12 01:50:34');
INSERT INTO `jy_save` VALUES ('3', '1', '2', '<xml>roles</xml>我的的 中文！！！！！', '2013-12-11 21:18:43', '2013-12-14 15:39:32');
INSERT INTO `jy_save` VALUES ('4', '2', '0', '<xml>roles</xml>', null, '2013-12-14 15:08:20');
INSERT INTO `jy_save` VALUES ('5', '2', '1', '<xml>roles</xml>', null, '2013-12-14 15:08:20');
INSERT INTO `jy_save` VALUES ('6', '2', '2', '我的', null, '2013-12-14 15:09:39');
INSERT INTO `jy_user` VALUES ('1', 'admin', '123456', '1000', '2013-12-11 21:17:59', '2013-12-11 21:18:01');
INSERT INTO `jy_user` VALUES ('2', 'chenggong', '123456', '1000', '2013-12-12 00:54:27', '2013-12-12 00:54:29');
