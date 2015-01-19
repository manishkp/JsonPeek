# JsonPeek and JsonPoke MS Build tasks

MsBuild tasks which allows reading and updating Json (in-memory or from disk File).
Supports MSbuild metadata, which allows one to read and update Json objects, arrays, etc.

Usage : 
## JSON Poke:
    1. <JsonPoke JsonInputPath="$(MSBuildProjectDirectory)\Project.json" JValue="Empty-FromTest1" JPath="Project.Name">
        </JsonPoke>
    2 a. <JsonPoke JsonInputPath="$(MSBuildProjectDirectory)\Project.json" JArray="@(TestArray1)" JPath="Project.TestArray"            Metadata="MyProp;Identity">
        </JsonPoke>
    2 b. <JsonPoke JsonInputPath="$(MSBuildProjectDirectory)\Project.json" JArray="t11.txt;t22.txt" JPath="Project.TestArray1">
    </JsonPoke>
        3. <JsonPoke JsonInputPath="$(MSBuildProjectDirectory)\Project.json" JObject="@(BuildNumber)" JPath="Project.TestObject" Metadata="Major;Minor;Build">
        <PropertyGroup>
          <JsonContent>
            <![CDATA[{ 
              "Projects":[
             { "Name": "P1",  "OutputFile": "P1.json",    "Variables": [  "Var1", "Var2" ]},
             {  "Name": "P2", "OutputFile": "P2.json",    "Variables": [  "Var1", "Var2"  ] } ] }
            ]]>
        </JsonContent>
      </PropertyGroup>

## JSON Peek: 
    <JsonPeek JPath="$.Projects" JsonContent="$(JsonContent)">    
      <Output TaskParameter="Result" ItemName="TestProjects" />
    </JsonPeek>
    <Message Text="Project.IncludedLibraryVariableSetIds[?(@.Name == 'Lib-69')].Value : @(Lib69Value)" />
    <Message Text="Project values: %(TestProjects.Name)" />
