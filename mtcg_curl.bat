@echo off

@REM REM --------------------------------------------------
@REM REM Monster Trading Cards Game
@REM REM --------------------------------------------------
@REM title Monster Trading Cards Game
@REM echo CURL Testing for Monster Trading Cards Game
@REM echo.
@REM REM --------------------------------------------------
@REM echo 1) Create Users (Registration)
@REM REM Create User
@REM set admin_token="Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiIyNmRhZGU1YS0wZmU3LTQ5MDgtYmM3OS01ZjkzNmNjYTY0NmYiLCJleHAiOjE3MzcxMzg5MTAsInVzZXJJZCI6IjI5OTE1Yjc2LTAyMWYtNGZjMC05YWQwLWI3NWMxMDc0Y2M5OCJ9.JhKDp6iMlhh0XAhv2Nr01xbunaut1r0tJrcXHIbcq44"
@REM curl -X POST http://localhost:9999/user --header "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiIyNmRhZGU1YS0wZmU3LTQ5MDgtYmM3OS01ZjkzNmNjYTY0NmYiLCJleHAiOjE3MzcxMzg5MTAsInVzZXJJZCI6IjI5OTE1Yjc2LTAyMWYtNGZjMC05YWQwLWI3NWMxMDc0Y2M5OCJ9.JhKDp6iMlhh0XAhv2Nr01xbunaut1r0tJrcXHIbcq44"  --header "Content-Type: application/json" -d "{\"name\":\"manuel\", \"password\":\"asdf\", \"role\":\"1\""}"
@REM echo.
@REM curl -X POST http://localhost:9999/user --header "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiIyNmRhZGU1YS0wZmU3LTQ5MDgtYmM3OS01ZjkzNmNjYTY0NmYiLCJleHAiOjE3MzcxMzg5MTAsInVzZXJJZCI6IjI5OTE1Yjc2LTAyMWYtNGZjMC05YWQwLWI3NWMxMDc0Y2M5OCJ9.JhKDp6iMlhh0XAhv2Nr01xbunaut1r0tJrcXHIbcq44"  --header "Content-Type: application/json" -d "{\"name\":\"peter\", \"password\":\"asdf\", \"role\":\"1\""}"
@REM echo. 
@REM echo.

@REM REM --------------------------------------------------
@REM echo 2) Login Users
@REM curl -X POST http://localhost:9999/login  --header "Content-Type: application/json" -d ""
@REM echo.
@REM curl -X POST http://localhost:9999/login  --header "Content-Type: application/json" -d "{\"name\":\"peter\", \"password\":\"asdf\"}"
@REM echo.
@REM echo.

@REM REM --------------------------------------------------
@REM echo 3) acquire packages manuel
@REM --header "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiIyNmRhZGU1YS0wZmU3LTQ5MDgtYmM3OS01ZjkzNmNjYTY0NmYiLCJleHAiOjE3MzcxNDQ4NTAsInVzZXJJZCI6ImM1ZjdmNWIxLTMxNjgtNDY3My05ODY1LTYzZTFjZWFkNDVlZiJ9.riyirAivnGLI3C76Ny3FseIq3Vn6tUUUtCqx48p7fhI"
@REM curl -X POST http://localhost:9999/transactions/package --header "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiIyNmRhZGU1YS0wZmU3LTQ5MDgtYmM3OS01ZjkzNmNjYTY0NmYiLCJleHAiOjE3MzcxNDQ4NTAsInVzZXJJZCI6ImM1ZjdmNWIxLTMxNjgtNDY3My05ODY1LTYzZTFjZWFkNDVlZiJ9.riyirAivnGLI3C76Ny3FseIq3Vn6tUUUtCqx48p7fhI"  --header "Content-Type: application/json" -d ""
@REM echo.
@REM echo.

@REM REM --------------------------------------------------
@REM echo 4) acquire packages peter
@REM curl -X POST http://localhost:9999/transactions/package --header "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiIyNmRhZGU1YS0wZmU3LTQ5MDgtYmM3OS01ZjkzNmNjYTY0NmYiLCJleHAiOjE3MzcxNDUyMjYsInVzZXJJZCI6IjkxMDM1NDliLTE5OTEtNGIyNi1iYTJhLWExNWQ4ZjZiODZjMyJ9.DNIQ9tNxBlh0LdFJkuk1xCQ_I_9PziVERVP4Gekk-w8"  --header "Content-Type: application/json" -d ""
@REM echo.
@REM echo.

@REM REM --------------------------------------------------
@REM echo 5) show all acquired cards manuel
@REM curl -X GET http://localhost:9999/stack --header "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiIyNmRhZGU1YS0wZmU3LTQ5MDgtYmM3OS01ZjkzNmNjYTY0NmYiLCJleHAiOjE3MzcxNDQ4NTAsInVzZXJJZCI6ImM1ZjdmNWIxLTMxNjgtNDY3My05ODY1LTYzZTFjZWFkNDVlZiJ9.riyirAivnGLI3C76Ny3FseIq3Vn6tUUUtCqx48p7fhI" -d ""
@REM echo.
@REM echo.

@REM REM --------------------------------------------------
@REM echo 6) show all acquired cards peter
@REM curl -X GET http://localhost:9999/stack --header "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiIyNmRhZGU1YS0wZmU3LTQ5MDgtYmM3OS01ZjkzNmNjYTY0NmYiLCJleHAiOjE3MzcxNDUyMjYsInVzZXJJZCI6IjkxMDM1NDliLTE5OTEtNGIyNi1iYTJhLWExNWQ4ZjZiODZjMyJ9.DNIQ9tNxBlh0LdFJkuk1xCQ_I_9PziVERVP4Gekk-w8" -d ""
@REM echo.
@REM echo.

REM --------------------------------------------------
echo 7) show unconfigured deck
echo manuel:
curl -X GET http://localhost:9999/deck --header "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiIyNmRhZGU1YS0wZmU3LTQ5MDgtYmM3OS01ZjkzNmNjYTY0NmYiLCJleHAiOjE3MzcxNDQ4NTAsInVzZXJJZCI6ImM1ZjdmNWIxLTMxNjgtNDY3My05ODY1LTYzZTFjZWFkNDVlZiJ9.riyirAivnGLI3C76Ny3FseIq3Vn6tUUUtCqx48p7fhI"
echo.
echo peter:
curl -X GET http://localhost:9999/deck --header "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiIyNmRhZGU1YS0wZmU3LTQ5MDgtYmM3OS01ZjkzNmNjYTY0NmYiLCJleHAiOjE3MzcxNDUyMjYsInVzZXJJZCI6IjkxMDM1NDliLTE5OTEtNGIyNi1iYTJhLWExNWQ4ZjZiODZjMyJ9.DNIQ9tNxBlh0LdFJkuk1xCQ_I_9PziVERVP4Gekk-w8"
echo.
echo.

REM --------------------------------------------------
echo 8) configure deck
echo manuel:
curl -X POST http://localhost:9999/deck --header "Content-Type: application/json" --header "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiIyNmRhZGU1YS0wZmU3LTQ5MDgtYmM3OS01ZjkzNmNjYTY0NmYiLCJleHAiOjE3MzcxNDQ4NTAsInVzZXJJZCI6ImM1ZjdmNWIxLTMxNjgtNDY3My05ODY1LTYzZTFjZWFkNDVlZiJ9.riyirAivnGLI3C76Ny3FseIq3Vn6tUUUtCqx48p7fhI" -d "{\"cards\": [11, 12, 13, 14]}"
echo.
curl -X GET http://localhost:9999/deck --header "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiIyNmRhZGU1YS0wZmU3LTQ5MDgtYmM3OS01ZjkzNmNjYTY0NmYiLCJleHAiOjE3MzcxNDQ4NTAsInVzZXJJZCI6ImM1ZjdmNWIxLTMxNjgtNDY3My05ODY1LTYzZTFjZWFkNDVlZiJ9.riyirAivnGLI3C76Ny3FseIq3Vn6tUUUtCqx48p7fhI"
echo.
echo peter:
curl -X POST http://localhost:9999/deck --header "Content-Type: application/json" --header "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiIyNmRhZGU1YS0wZmU3LTQ5MDgtYmM3OS01ZjkzNmNjYTY0NmYiLCJleHAiOjE3MzcxNDQ4NTAsInVzZXJJZCI6ImM1ZjdmNWIxLTMxNjgtNDY3My05ODY1LTYzZTFjZWFkNDVlZiJ9.riyirAivnGLI3C76Ny3FseIq3Vn6tUUUtCqx48p7fhI" -d "{\"cards\": [3, 4, 5, 2]}"
echo.
curl -X GET http://localhost:9999/deck --header "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiIyNmRhZGU1YS0wZmU3LTQ5MDgtYmM3OS01ZjkzNmNjYTY0NmYiLCJleHAiOjE3MzcxNDUyMjYsInVzZXJJZCI6IjkxMDM1NDliLTE5OTEtNGIyNi1iYTJhLWExNWQ4ZjZiODZjMyJ9.DNIQ9tNxBlh0LdFJkuk1xCQ_I_9PziVERVP4Gekk-w8"
echo.
echo.