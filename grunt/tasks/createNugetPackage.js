module.exports = function (grunt) {

  grunt.registerTask('createNugetPackage',
     'create package ', function () {
    var
      //fs = require('fs'),
      //S = require('string'),
      string = require('string-formatter'),
      os = grunt.config('os'),
      config = grunt.config('config'),
      //configuration = grunt.config('msbuild_configuration'),
      nuget_path = os === 'win' ?
        config.win.nuget : config.linux.nuget,
      tasks = [];

    function createPackageWithTitle(title) {
      var
        dir_path = string.format('{0}/../../{1}/', __dirname, title),
        csproj = string.format('{0}{1}.csproj', dir_path, title),
        pack = string.format('{0} pack {1}', nuget_path, csproj);

      tasks.push(pack);
    }

    if (os === 'win') {
      createPackageWithTitle('EngineIoClientDotNet');
    } 

    grunt.log.writeln('tasks = %s', JSON.stringify(tasks));
    grunt.config('shell.exec.command', tasks.join('&&'));
    grunt.task.run('shell');       
  });
};