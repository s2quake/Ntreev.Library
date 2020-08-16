@echo off
powershell -executionpolicy remotesigned -File %~dp0\build.ps1 -BuildPath %~dp0 -PropsPath "base.props" -Force