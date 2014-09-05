module.exports = function (grunt) {

  grunt.registerTask('createNugetPackage',
     'create package ', function () {
    var
      fs = require('fs'),
      S = require('string'),
      string = require('string-formatter'),
      os = grunt.config('os'),
      config = grunt.config('config'),
      working_path = grunt.config('working_path'),
      //configuration = grunt.config('msbuild_configuration'),
      configuration = grunt.config('msbuild_configuration'),
      output_path_base = 'bin\\' + configuration + '\\',
      nuget_builds = grunt.config('nuget_builds'),
      nuget_path = os === 'win' ?
        config.win.nuget : config.linux.nuget,
      dst_path,
      template_file_content,
      i,
      files;

    //function createPackageWithTitle(title) {
    //  var
    //    dir_path = string.format('{0}/../../{1}/', __dirname, title),
    //    csproj = string.format('{0}{1}.csproj', dir_path, title),
    //    pack = string.format('{0} pack {1}', nuget_path, csproj);

    //  tasks.push(pack);
    //}

    if (os !== 'win') {
      return;
    } 

    //createPackageWithTitle('EngineIoClientDotNet');


    if (! fs.existsSync(working_path)) {
      fs.mkdirSync(working_path);
    }
    for (i = 0; i < nuget_builds.length; i++) {
      dst_path = working_path + nuget_builds[i].NuGetDir + '/';
      //files = fs.readdirSync(dst_path);
      grunt.log.writeln(string.format('dst_path={0}', dst_path));
      fs.mkdirSync(dst_path);
    }
    

    template_file_content = fs.readFileSync('./templates/EngineIoClientDotNet.nuspec');
    template_file_content = S(template_file_content).replaceAll('@VERSION@', config.version).s;
    fs.writeFileSync(string.format('{0}EngineIoClientDotNet.nuspec', working_path), template_file_content);



    function addBuildWithTitle(title, dir) {
      var
        src_path = string.format('{0}/../../src/EngineIoClientDotNet/{1}{2}/', __dirname, output_path_base, dir),
        dst_path = working_path + dir + '/',
        src_file = src_path + string.format('{0}.dll',title),
        dst_file = dst_path + string.format('{0}.dll',title);
      
      grunt.log.writeln(string.format('src_file={0} dst_file={1}', src_file, dst_file));
      fs.writeFileSync(dst_file, fs.readFileSync(src_file));

      src_file = src_path + string.format('{0}.pdb', title);
      dst_file = dst_path + string.format('{0}.pdb', title);

      grunt.log.writeln(string.format('src_file={0} dst_file={1}', src_file, dst_file));
      fs.writeFileSync(dst_file, fs.readFileSync(src_file));
    }

    for (i = 0; i < nuget_builds.length; i++) {
      addBuildWithTitle(nuget_builds[i].Name, nuget_builds[i].NuGetDir);
    }

    //grunt.log.writeln('tasks = %s', JSON.stringify(tasks));
    //grunt.config('shell.exec.command', tasks.join('&&'));
    //grunt.task.run('shell');       
  });
};