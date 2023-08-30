# 说明
这是我的`网络管理`课程的课程实验作品，也是我的第一个WinUI3作品，同时也是我的第一个C#作品。如果我的代码有任何可以改进的地方，你都可以向我提建议，帮助我学习
# 功能
基于SNMP协议的数据浏览（MIB Browser中的SNMP Get功能）
 - Get Value: 获取目标主机对应OID的值
 - Get Next: 获取下一条OID的值
 - Get Bulk: 批量获取MaxRepetition条MIB叶子节点数据
 - Get Tree: 基于Bulk开发。可以获取给定MIB节点下子树中的所有叶子节点的值
 - Scan Hosts: 使用ICMP进行主机发现，并使用SNMP获取主机名称
# 开发环境
- Visual Studio 2022 ver 17.6.4
- WindowsAppSDK(WinUI3) ver 1.3.230602002+ (Latest 1.4.230822000)
- CommunityToolkit.Mvvm ver 8.2.1
# 新进展
1. 采用MVVM开发模式重构了代码。(从造轮子到使用CommunityToolkit)
2. 采用了全新MicaAlt材质作为背景。
3. 去除了任务栏烦人的背景色。
# 未来规划
1. 持续完善功能