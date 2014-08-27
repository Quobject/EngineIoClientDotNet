module.exports = function (grunt) {

  grunt.registerTask('buildClient',
      'build cs modules', function () {
    var
      os = grunt.config('os'),
      tasks = [],
      cs_sln = __dirname + '/../../EngineIoClientDotNet/EngineIoClientDotNet.csproj',
      build_format_str;



    if (os === 'win') {
      build_format_str = 'C:/WINDOWS/System32/WindowsPowerShell/v1.0/powershell.exe start-process -NoNewWindow ' +
            '-FilePath C:/Windows/Microsoft.NET/Framework/v4.0.30319/msbuild.exe ' +
            '-ArgumentList \' {0}  /p:Configuration=Release \' ';
    } else {
      build_format_str = "xbuild {0} /p:Configuration=Release";
    }

    grunt.log.writeln('cs_sln = "%s"', cs_sln);
    tasks.push(build_format_str.format(cs_sln));

    grunt.log.writeln('tasks = %s', JSON.stringify(tasks));
    //grunt.config('shell.exec.command', tasks.join('&&'));
    //grunt.task.run('shell');

  });
};


