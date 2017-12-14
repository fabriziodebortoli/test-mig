// Copyright (c) 2016 The Chromium Embedded Framework Authors. All rights
// reserved. Use of this source code is governed by a BSD-style license that
// can be found in the LICENSE file.
//
// ---------------------------------------------------------------------------
//
// This file was generated by the CEF translator tool. If making changes by
// hand only do so within the body of existing method and function
// implementations. See the translator.README.txt file in the tools directory
// for more information.
//

#include "include/cef_version.h"
#include "libcef_dll/ctocpp/command_line_ctocpp.h"
#include "libcef_dll/transfer_util.h"


// STATIC METHODS - Body may be edited by hand.

CefRefPtr<CefCommandLine> CefCommandLine::CreateCommandLine() {
  const char* api_hash = cef_api_hash(0);
  if (strcmp(api_hash, CEF_API_HASH_PLATFORM)) {
    // The libcef API hash does not match the current header API hash.
    NOTREACHED();
    return NULL;
  }

  // AUTO-GENERATED CONTENT - DELETE THIS COMMENT BEFORE MODIFYING

  // Execute
  cef_command_line_t* _retval = cef_command_line_create();

  // Return type: refptr_same
  return CefCommandLineCToCpp::Wrap(_retval);
}

CefRefPtr<CefCommandLine> CefCommandLine::GetGlobalCommandLine() {
  const char* api_hash = cef_api_hash(0);
  if (strcmp(api_hash, CEF_API_HASH_PLATFORM)) {
    // The libcef API hash does not match the current header API hash.
    NOTREACHED();
    return NULL;
  }

  // AUTO-GENERATED CONTENT - DELETE THIS COMMENT BEFORE MODIFYING

  // Execute
  cef_command_line_t* _retval = cef_command_line_get_global();

  // Return type: refptr_same
  return CefCommandLineCToCpp::Wrap(_retval);
}


// VIRTUAL METHODS - Body may be edited by hand.

bool CefCommandLineCToCpp::IsValid() {
  cef_command_line_t* _struct = GetStruct();
  if (CEF_MEMBER_MISSING(_struct, is_valid))
    return false;

  // AUTO-GENERATED CONTENT - DELETE THIS COMMENT BEFORE MODIFYING

  // Execute
  int _retval = _struct->is_valid(_struct);

  // Return type: bool
  return _retval?true:false;
}

bool CefCommandLineCToCpp::IsReadOnly() {
  cef_command_line_t* _struct = GetStruct();
  if (CEF_MEMBER_MISSING(_struct, is_read_only))
    return false;

  // AUTO-GENERATED CONTENT - DELETE THIS COMMENT BEFORE MODIFYING

  // Execute
  int _retval = _struct->is_read_only(_struct);

  // Return type: bool
  return _retval?true:false;
}

CefRefPtr<CefCommandLine> CefCommandLineCToCpp::Copy() {
  cef_command_line_t* _struct = GetStruct();
  if (CEF_MEMBER_MISSING(_struct, copy))
    return NULL;

  // AUTO-GENERATED CONTENT - DELETE THIS COMMENT BEFORE MODIFYING

  // Execute
  cef_command_line_t* _retval = _struct->copy(_struct);

  // Return type: refptr_same
  return CefCommandLineCToCpp::Wrap(_retval);
}

void CefCommandLineCToCpp::InitFromArgv(int argc, const char* const* argv) {
  cef_command_line_t* _struct = GetStruct();
  if (CEF_MEMBER_MISSING(_struct, init_from_argv))
    return;

  // AUTO-GENERATED CONTENT - DELETE THIS COMMENT BEFORE MODIFYING

  // Verify param: argv; type: simple_byaddr
  DCHECK(argv);
  if (!argv)
    return;

  // Execute
  _struct->init_from_argv(_struct,
      argc,
      argv);
}

void CefCommandLineCToCpp::InitFromString(const CefString& command_line) {
  cef_command_line_t* _struct = GetStruct();
  if (CEF_MEMBER_MISSING(_struct, init_from_string))
    return;

  // AUTO-GENERATED CONTENT - DELETE THIS COMMENT BEFORE MODIFYING

  // Verify param: command_line; type: string_byref_const
  DCHECK(!command_line.empty());
  if (command_line.empty())
    return;

  // Execute
  _struct->init_from_string(_struct,
      command_line.GetStruct());
}

void CefCommandLineCToCpp::Reset() {
  cef_command_line_t* _struct = GetStruct();
  if (CEF_MEMBER_MISSING(_struct, reset))
    return;

  // AUTO-GENERATED CONTENT - DELETE THIS COMMENT BEFORE MODIFYING

  // Execute
  _struct->reset(_struct);
}

void CefCommandLineCToCpp::GetArgv(std::vector<CefString>& argv) {
  cef_command_line_t* _struct = GetStruct();
  if (CEF_MEMBER_MISSING(_struct, get_argv))
    return;

  // AUTO-GENERATED CONTENT - DELETE THIS COMMENT BEFORE MODIFYING

  // Translate param: argv; type: string_vec_byref
  cef_string_list_t argvList = cef_string_list_alloc();
  DCHECK(argvList);
  if (argvList)
    transfer_string_list_contents(argv, argvList);

  // Execute
  _struct->get_argv(_struct,
      argvList);

  // Restore param:argv; type: string_vec_byref
  if (argvList) {
    argv.clear();
    transfer_string_list_contents(argvList, argv);
    cef_string_list_free(argvList);
  }
}

CefString CefCommandLineCToCpp::GetCommandLineString() {
  cef_command_line_t* _struct = GetStruct();
  if (CEF_MEMBER_MISSING(_struct, get_command_line_string))
    return CefString();

  // AUTO-GENERATED CONTENT - DELETE THIS COMMENT BEFORE MODIFYING

  // Execute
  cef_string_userfree_t _retval = _struct->get_command_line_string(_struct);

  // Return type: string
  CefString _retvalStr;
  _retvalStr.AttachToUserFree(_retval);
  return _retvalStr;
}

CefString CefCommandLineCToCpp::GetProgram() {
  cef_command_line_t* _struct = GetStruct();
  if (CEF_MEMBER_MISSING(_struct, get_program))
    return CefString();

  // AUTO-GENERATED CONTENT - DELETE THIS COMMENT BEFORE MODIFYING

  // Execute
  cef_string_userfree_t _retval = _struct->get_program(_struct);

  // Return type: string
  CefString _retvalStr;
  _retvalStr.AttachToUserFree(_retval);
  return _retvalStr;
}

void CefCommandLineCToCpp::SetProgram(const CefString& program) {
  cef_command_line_t* _struct = GetStruct();
  if (CEF_MEMBER_MISSING(_struct, set_program))
    return;

  // AUTO-GENERATED CONTENT - DELETE THIS COMMENT BEFORE MODIFYING

  // Verify param: program; type: string_byref_const
  DCHECK(!program.empty());
  if (program.empty())
    return;

  // Execute
  _struct->set_program(_struct,
      program.GetStruct());
}

bool CefCommandLineCToCpp::HasSwitches() {
  cef_command_line_t* _struct = GetStruct();
  if (CEF_MEMBER_MISSING(_struct, has_switches))
    return false;

  // AUTO-GENERATED CONTENT - DELETE THIS COMMENT BEFORE MODIFYING

  // Execute
  int _retval = _struct->has_switches(_struct);

  // Return type: bool
  return _retval?true:false;
}

bool CefCommandLineCToCpp::HasSwitch(const CefString& name) {
  cef_command_line_t* _struct = GetStruct();
  if (CEF_MEMBER_MISSING(_struct, has_switch))
    return false;

  // AUTO-GENERATED CONTENT - DELETE THIS COMMENT BEFORE MODIFYING

  // Verify param: name; type: string_byref_const
  DCHECK(!name.empty());
  if (name.empty())
    return false;

  // Execute
  int _retval = _struct->has_switch(_struct,
      name.GetStruct());

  // Return type: bool
  return _retval?true:false;
}

CefString CefCommandLineCToCpp::GetSwitchValue(const CefString& name) {
  cef_command_line_t* _struct = GetStruct();
  if (CEF_MEMBER_MISSING(_struct, get_switch_value))
    return CefString();

  // AUTO-GENERATED CONTENT - DELETE THIS COMMENT BEFORE MODIFYING

  // Verify param: name; type: string_byref_const
  DCHECK(!name.empty());
  if (name.empty())
    return CefString();

  // Execute
  cef_string_userfree_t _retval = _struct->get_switch_value(_struct,
      name.GetStruct());

  // Return type: string
  CefString _retvalStr;
  _retvalStr.AttachToUserFree(_retval);
  return _retvalStr;
}

void CefCommandLineCToCpp::GetSwitches(SwitchMap& switches) {
  cef_command_line_t* _struct = GetStruct();
  if (CEF_MEMBER_MISSING(_struct, get_switches))
    return;

  // AUTO-GENERATED CONTENT - DELETE THIS COMMENT BEFORE MODIFYING

  // Translate param: switches; type: string_map_single_byref
  cef_string_map_t switchesMap = cef_string_map_alloc();
  DCHECK(switchesMap);
  if (switchesMap)
    transfer_string_map_contents(switches, switchesMap);

  // Execute
  _struct->get_switches(_struct,
      switchesMap);

  // Restore param:switches; type: string_map_single_byref
  if (switchesMap) {
    switches.clear();
    transfer_string_map_contents(switchesMap, switches);
    cef_string_map_free(switchesMap);
  }
}

void CefCommandLineCToCpp::AppendSwitch(const CefString& name) {
  cef_command_line_t* _struct = GetStruct();
  if (CEF_MEMBER_MISSING(_struct, append_switch))
    return;

  // AUTO-GENERATED CONTENT - DELETE THIS COMMENT BEFORE MODIFYING

  // Verify param: name; type: string_byref_const
  DCHECK(!name.empty());
  if (name.empty())
    return;

  // Execute
  _struct->append_switch(_struct,
      name.GetStruct());
}

void CefCommandLineCToCpp::AppendSwitchWithValue(const CefString& name,
    const CefString& value) {
  cef_command_line_t* _struct = GetStruct();
  if (CEF_MEMBER_MISSING(_struct, append_switch_with_value))
    return;

  // AUTO-GENERATED CONTENT - DELETE THIS COMMENT BEFORE MODIFYING

  // Verify param: name; type: string_byref_const
  DCHECK(!name.empty());
  if (name.empty())
    return;
  // Verify param: value; type: string_byref_const
  DCHECK(!value.empty());
  if (value.empty())
    return;

  // Execute
  _struct->append_switch_with_value(_struct,
      name.GetStruct(),
      value.GetStruct());
}

bool CefCommandLineCToCpp::HasArguments() {
  cef_command_line_t* _struct = GetStruct();
  if (CEF_MEMBER_MISSING(_struct, has_arguments))
    return false;

  // AUTO-GENERATED CONTENT - DELETE THIS COMMENT BEFORE MODIFYING

  // Execute
  int _retval = _struct->has_arguments(_struct);

  // Return type: bool
  return _retval?true:false;
}

void CefCommandLineCToCpp::GetArguments(ArgumentList& arguments) {
  cef_command_line_t* _struct = GetStruct();
  if (CEF_MEMBER_MISSING(_struct, get_arguments))
    return;

  // AUTO-GENERATED CONTENT - DELETE THIS COMMENT BEFORE MODIFYING

  // Translate param: arguments; type: string_vec_byref
  cef_string_list_t argumentsList = cef_string_list_alloc();
  DCHECK(argumentsList);
  if (argumentsList)
    transfer_string_list_contents(arguments, argumentsList);

  // Execute
  _struct->get_arguments(_struct,
      argumentsList);

  // Restore param:arguments; type: string_vec_byref
  if (argumentsList) {
    arguments.clear();
    transfer_string_list_contents(argumentsList, arguments);
    cef_string_list_free(argumentsList);
  }
}

void CefCommandLineCToCpp::AppendArgument(const CefString& argument) {
  cef_command_line_t* _struct = GetStruct();
  if (CEF_MEMBER_MISSING(_struct, append_argument))
    return;

  // AUTO-GENERATED CONTENT - DELETE THIS COMMENT BEFORE MODIFYING

  // Verify param: argument; type: string_byref_const
  DCHECK(!argument.empty());
  if (argument.empty())
    return;

  // Execute
  _struct->append_argument(_struct,
      argument.GetStruct());
}

void CefCommandLineCToCpp::PrependWrapper(const CefString& wrapper) {
  cef_command_line_t* _struct = GetStruct();
  if (CEF_MEMBER_MISSING(_struct, prepend_wrapper))
    return;

  // AUTO-GENERATED CONTENT - DELETE THIS COMMENT BEFORE MODIFYING

  // Verify param: wrapper; type: string_byref_const
  DCHECK(!wrapper.empty());
  if (wrapper.empty())
    return;

  // Execute
  _struct->prepend_wrapper(_struct,
      wrapper.GetStruct());
}


// CONSTRUCTOR - Do not edit by hand.

CefCommandLineCToCpp::CefCommandLineCToCpp() {
}

template<> cef_command_line_t* CefCToCpp<CefCommandLineCToCpp, CefCommandLine,
    cef_command_line_t>::UnwrapDerived(CefWrapperType type,
    CefCommandLine* c) {
  NOTREACHED() << "Unexpected class type: " << type;
  return NULL;
}

#ifndef NDEBUG
template<> base::AtomicRefCount CefCToCpp<CefCommandLineCToCpp, CefCommandLine,
    cef_command_line_t>::DebugObjCt = 0;
#endif

template<> CefWrapperType CefCToCpp<CefCommandLineCToCpp, CefCommandLine,
    cef_command_line_t>::kWrapperType = WT_COMMAND_LINE;
